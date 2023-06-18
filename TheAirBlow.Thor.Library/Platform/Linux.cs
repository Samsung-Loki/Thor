using System.Runtime.InteropServices;
using Serilog;
using TheAirBlow.Thor.Library.Communication;

namespace TheAirBlow.Thor.Library.Platform;

public class Linux : IHandler, IDisposable {
    public string GetNotes()
        => "You have to run Thor as root or edit udev rules as follows:\n" +
           "1) create and open /etc/udev/rules.d/51-android.rules in an editor\n" +
           "2) enter SUBSYSTEM==\"usb\", ATTR{idVendor}==\"04e8\", MODE=\"0666\", GROUP=\"YourUserGroupHere\"\n" +
           "Additionally, you may have to disable the cdc_acm kernel module:\n" +
           "1) To temporarily unload, run \"sudo modprobe -r cdc_acm\"\n" +
           "2) To disable it, run \"echo 'blacklist cdc_acm' | sudo tee -a /etc/modprobe.d/cdc_acm-blacklist.conf\"";

    public async Task<List<DeviceInfo>> GetDevices() {
        var list = new List<DeviceInfo>();
        foreach (var bus in Directory.EnumerateDirectories("/dev/bus/usb/"))
        foreach (var device in Directory.EnumerateFiles(bus)) {
            try {
                using var file = new FileStream(device, FileMode.Open, FileAccess.Read);
                using var reader = new BinaryReader(file);
                // USB_DT_DEVICE usb_device_descriptor struct:
                // https://github.com/torvalds/linux/blob/master/include/uapi/linux/usb/ch9.h#L289
                file.Seek(1, SeekOrigin.Current);
                if (reader.ReadByte() != 0x01)
                    continue; // Not USB_DT_DEVICE
                file.Seek(6, SeekOrigin.Current);
                var vendor = reader.ReadInt16();
                if (vendor == USB.Vendor) {
                    var product = reader.ReadInt16();
                    list.Add(new DeviceInfo {
                        DisplayName = await Lookup.GetDisplayName(vendor, product),
                        Identifier = device[13..].Replace("/", ":")
                    });
                }
            } catch { /* Ignored */ }
        }

        return list;
    }

    private uint _interface;
    private uint? _readEndpoint;
    private uint? _writeEndpoint;
    private int? _deviceFd;
    private bool _connected;
    private bool _detached;
    private bool _writeZlp;

    public void Initialize(string id) {
        id = id.Replace(":", "/");
        var path = $"/dev/bus/usb/{id}";
        if (!File.Exists(path))
            throw new InvalidOperationException("Device disconnected?");
        var found = false;
        using (var file = new FileStream(path, FileMode.Open, FileAccess.Read)) {
            using var reader = new BinaryReader(file);
            // USB_DT_DEVICE usb_device_descriptor struct:
            // https://github.com/torvalds/linux/blob/master/include/uapi/linux/usb/ch9.h#L289
            file.Seek(1, SeekOrigin.Current);
            if (reader.ReadByte() != 0x01)
                throw new InvalidDataException("USB_DT_DEVICE assertion fail!");
            file.Seek(6, SeekOrigin.Current);
            if (reader.ReadInt16() != USB.Vendor)
                throw new InvalidDataException("This is not a Samsung device!");
            file.Seek(8, SeekOrigin.Current);
            // USB_DT_CONFIG usb_config_descriptor struct:
            // https://github.com/torvalds/linux/blob/master/include/uapi/linux/usb/ch9.h#L349
            file.Seek(1, SeekOrigin.Current);
            if (reader.ReadByte() != 0x02)
                throw new InvalidDataException("USB_DT_CONFIG assertion fail!");
            file.Seek(2, SeekOrigin.Current);
            var numInterfaces = (int)reader.ReadByte();
            file.Seek(4, SeekOrigin.Current);
            Log.Debug("Number of interfaces: {0}", numInterfaces);
            // USB_DT_INTERFACE usb_interface_descriptor struct:
            // https://github.com/torvalds/linux/blob/master/include/uapi/linux/usb/ch9.h#L388
            for (var i = 0; i < numInterfaces; i++) {
                Log.Debug("~~~~ Interface index {0} ~~~~", i);
                file.Seek(1, SeekOrigin.Current);
                if (reader.ReadByte() != 0x04)
                    throw new InvalidDataException("USB_DT_INTERFACE assertion fail!");
                _interface = reader.ReadByte();
                file.Seek(1, SeekOrigin.Current);
                var numEndpoints = (int)reader.ReadByte();
                Log.Debug("Number of endpoints: {0}", numEndpoints);
                var clss = reader.ReadByte();
                Log.Debug("Interface class: 0x{0:X2}", clss);
                if (clss == 0x02) {
                    Log.Debug("!! Found USB_CLASS_COMM");
                    file.Seek(19, SeekOrigin.Current);
                }
                file.Seek(3, SeekOrigin.Current);
                // USB_DT_ENDPOINT usb_endpoint_descriptor struct:
                // https://github.com/torvalds/linux/blob/master/include/uapi/linux/usb/ch9.h#L407
                _readEndpoint = null; _writeEndpoint = null;
                for (var j = 0; j < numEndpoints; j++) {
                    Log.Debug("$$ Endpoint index {0} $$", j);
                    file.Seek(1, SeekOrigin.Current);
                    if (reader.ReadByte() != 0x05)
                        throw new InvalidDataException("USB_DT_ENDPOINT assertion fail!");
                    var addr = reader.ReadByte();
                    Log.Debug("Endpoint address: 0x{0:X2}", addr);

                    if ((reader.ReadByte() & 0x03) != 0x02) {
                        file.Seek(3, SeekOrigin.Current);
                        Log.Debug("!! USB_ENDPOINT_XFER_BULK fail");
                        continue; // Not USB_ENDPOINT_XFER_BULK
                    }
                    
                    if (addr > 0x80)
                        _readEndpoint = addr;
                    else _writeEndpoint = addr;
                    file.Seek(3, SeekOrigin.Current);
                }
                
                // Class is USB_CLASS_CDC_DATA, found valid read and write endpoints
                found = clss == 0x0a && _readEndpoint.HasValue && _writeEndpoint.HasValue;
                if (found) {
                    Log.Debug("> Interface is valid, exiting");
                    break;
                } 
                
                Log.Debug("!! Interface is invalid, continuing");
            }
        }

        if (!found) throw new InvalidOperationException("Failed to find valid endpoints!");
        Log.Debug("Interface: 0x{0:X2}, Read Endpoint: 0x{1:X2}, Write Endpoint: 0x{2:X2}",
            _interface, _readEndpoint, _writeEndpoint);
        
        // Create a device file handle
        if ((_deviceFd = Interop.Open(path, Interop.O_RDWR)) < 0) {
            var error = Marshal.GetLastWin32Error();
            var ptr = Interop.StrError(error);
            var str = Marshal.PtrToStringUTF8(ptr);
            throw new InvalidOperationException($"Failed to open the device for RW: {str} ({error})");
        }
        
        // Detach kernel driver if present
        var driver = new Interop.GetDriver {
            Interface = (int)_interface
        };
        
        if (Interop.IoCtl(_deviceFd.Value, Interop.USBDEVFS_GETDRIVER, ref driver) > 0) {
            var ioctl = new Interop.UsbIoCtl {
                CommandCode = (int)Interop.USBDEVFS_DISCONNECT,
                Interface = (int)_interface,
                Data = nint.Zero
            };

            if (Interop.IoCtl(_deviceFd.Value, Interop.USBDEVFS_IOCTL, ref ioctl) < 0) {
                var error = Marshal.GetLastWin32Error();
                var ptr = Interop.StrError(error);
                var str = Marshal.PtrToStringUTF8(ptr);
                throw new InvalidOperationException($"Failed to detach kernel driver: {str} ({error})");
            }
            
            _detached = true;
        }

        // Claim interface
        if (Interop.IoCtl(_deviceFd.Value, Interop.USBDEVFS_CLAIMINTERFACE, ref _interface) < 0) {
            var error = Marshal.GetLastWin32Error();
            var ptr = Interop.StrError(error);
            var str = Marshal.PtrToStringUTF8(ptr);
            throw new InvalidOperationException($"Failed to claim interface: {str} ({error})");
        }

        _connected = true;
    }

    public bool IsConnected()
        => _connected;

    public void Disconnect()
        => Dispose();

    public unsafe void BulkWrite(byte[] buf, int timeout = 5000, bool zlp = false) {
        if (!_connected)
            throw new InvalidOperationException("Not connected to a device!");
        fixed (void* p = buf) {
            var bufPtr = (nint)p;
            var bulk = new Interop.BulkTransfer {
                Endpoint = _writeEndpoint!.Value,
                Length = (uint)buf.Length,
                Timeout = (uint)timeout,
                Data = bufPtr
            };
            
            if (Interop.IoCtl(_deviceFd!.Value, Interop.USBDEVFS_BULK, ref bulk) < 0) {
                var error = Marshal.GetLastWin32Error();
                var ptr = Interop.StrError(error);
                var str = Marshal.PtrToStringUTF8(ptr);
                throw new InvalidOperationException($"Failed to bulk write: {str} ({error})");
            }
        }

        // Write ZLP, disable if failed
        if (_writeZlp && !zlp) {
            try {
                SendZLP();
            } catch {
                _writeZlp = false;
            }
        }
    }

    public unsafe byte[] BulkRead(int amount, out int read, int timeout = 5000) {
        if (!_connected)
            throw new InvalidOperationException("Not connected to a device!");

        var buf = new byte[amount];
        fixed (void* p = buf) {
            var bufPtr = (nint)p;
            var bulk = new Interop.BulkTransfer {
                Endpoint = _readEndpoint!.Value,
                Timeout = (uint)timeout,
                Length = (uint)amount,
                Data = bufPtr
            };
            
            if ((read = Interop.IoCtl(_deviceFd!.Value, Interop.USBDEVFS_BULK, ref bulk)) < 0) {
                var error = Marshal.GetLastWin32Error();
                var ptr = Interop.StrError(error);
                var str = Marshal.PtrToStringUTF8(ptr);
                throw new InvalidOperationException($"Failed to bulk read: {str} ({error})");
            }
            
            var arr = new byte[read];
            Marshal.Copy(bufPtr, arr, 0, read);
            return arr;
        }
    }

    public void SendZLP() {
        if (!_connected)
            throw new InvalidOperationException("Not connected to a device!");
        BulkWrite(Array.Empty<byte>(), 100, true);
    }

    public void ReadZLP() {
        if (!_connected)
            throw new InvalidOperationException("Not connected to a device!");
        BulkRead(0, out _, 100);
    }
    
    public void Dispose() {
        // Release the interface
        if (_connected && _deviceFd.HasValue)
            if (Interop.IoCtl(_deviceFd.Value, Interop.USBDEVFS_RELEASEINTERFACE, ref _interface) < 0) {
                var error = Marshal.GetLastWin32Error();
                var ptr = Interop.StrError(error);
                var str = Marshal.PtrToStringUTF8(ptr);
                Log.Fatal("Failed to release interface, {0} ({1})", str, error);
            }

        // Attach the kernel driver back
        if (_detached) {
            var ioctl = new Interop.UsbIoCtl {
                CommandCode = (int)Interop.USBDEVFS_CONNECT,
                Interface = (int)_interface,
                Data = nint.Zero
            };
            if (Interop.IoCtl(_deviceFd.Value, Interop.USBDEVFS_IOCTL, ref ioctl) < 0) {
                var error = Marshal.GetLastWin32Error();
                var ptr = Interop.StrError(error);
                var str = Marshal.PtrToStringUTF8(ptr);
                throw new InvalidOperationException($"Failed to attach kernel driver: {str} ({error})");
            }
        }
        
        // Close device file handle
        if (_deviceFd.HasValue)
            if (Interop.Close(_deviceFd.Value) < 0) {
                var error = Marshal.GetLastWin32Error();
                var ptr = Interop.StrError(error);
                var str = Marshal.PtrToStringUTF8(ptr);
                Log.Fatal("Failed to close file descriptor: {0} ({1})", str, error);
            }

        _connected = false;
    }

    public static unsafe class Interop {
        private const int _IOC_NRBITS = 8;
        private const int _IOC_TYPEBITS = 8;
        private const int _IOC_SIZEBITS = 14;
        private static readonly int _IOC_NRSHIFT = 0;
        private static readonly int _IOC_TYPESHIFT = _IOC_NRSHIFT + _IOC_NRBITS;
        private static readonly int _IOC_SIZESHIFT = _IOC_TYPESHIFT + _IOC_TYPEBITS;
        private static readonly int _IOC_DIRSHIFT = _IOC_SIZESHIFT + _IOC_SIZEBITS;
        private static uint _IO(uint type, uint nr)
            => (0U << _IOC_DIRSHIFT) | (type << _IOC_TYPESHIFT) 
               | (nr << _IOC_NRSHIFT) | (0U << _IOC_SIZESHIFT);
        private static uint _IOWR(uint type, uint nr, uint size)
            => ((1U | 2U) << _IOC_DIRSHIFT) | (type << _IOC_TYPESHIFT) 
               | (nr << _IOC_NRSHIFT) | (size << _IOC_SIZESHIFT);
        private static uint _IOR(uint type, uint nr, uint size)
            => (2U << _IOC_DIRSHIFT) | (type << _IOC_TYPESHIFT) 
               | (nr << _IOC_NRSHIFT) | (size << _IOC_SIZESHIFT);
        private static uint _IOW(uint type, uint nr, uint size)
            => (1U << _IOC_DIRSHIFT) | (type << _IOC_TYPESHIFT) 
               | (nr << _IOC_NRSHIFT) | (size << _IOC_SIZESHIFT);
        public static uint USBDEVFS_BULK = _IOWR('U', 2, (uint)sizeof(BulkTransfer));
        public static uint USBDEVFS_IOCTL = _IOWR('U', 18, (uint)sizeof(UsbIoCtl));
        public static uint USBDEVFS_CLAIMINTERFACE = _IOR('U', 15, sizeof(uint));
        public static uint USBDEVFS_RELEASEINTERFACE = _IOR('U', 16, sizeof(uint));
        public static uint USBDEVFS_GETDRIVER = _IOW('U', 8, (uint)sizeof(GetDriver));
        public static uint USBDEVFS_DISCONNECT = _IO('U', 22);
        public static uint USBDEVFS_CONNECT = _IO('U', 23);

        public struct BulkTransfer {
            public uint Endpoint;
            public uint Length;
            public uint Timeout;
            public nint Data;
        }

        public struct GetDriver {
            public int Interface;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] Driver;
        }
        
        public struct UsbIoCtl {
            public int Interface;
            public int CommandCode;
            public nint Data;
        }
        
        public const int O_RDWR = 0x02;
        [DllImport("libc", EntryPoint = "open", SetLastError = true)]
        public static extern int Open([MarshalAs(UnmanagedType.LPStr)] 
            string path, int flags);
        
        [DllImport("libc", EntryPoint = "close", SetLastError = true)]
        public static extern int Close(int fd);
        
        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        public static extern int IoCtl(int fd, ulong request, ref BulkTransfer bulk);
        
        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        public static extern int IoCtl(int fd, ulong request, ref UsbIoCtl ioctl);
        
        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        public static extern int IoCtl(int fd, ulong request, ref GetDriver driver);
        
        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        public static extern int IoCtl(int fd, ulong request, ref uint iface);
        
        [DllImport("libc", EntryPoint = "strerror")]
        public static extern nint StrError(int code);
    }
}