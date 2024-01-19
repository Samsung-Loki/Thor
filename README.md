![badges](https://img.shields.io/github/contributors/Samsung-Loki/Thor.svg)
![badges](https://img.shields.io/github/forks/Samsung-Loki/Thor.svg)
![badges](https://img.shields.io/github/stars/Samsung-Loki/Thor.svg)
![badges](https://img.shields.io/github/issues/Samsung-Loki/Thor.svg)
# Thor Flash Utility
This is a flash utility for Samsung devices, made from scratch in C#. \
To run this, you must have .NET 7 runtime installed on your computer. \
**This tool was tested and works flawlessly, I'm open for any bug reports.**

## Disclaimer
1) You can't reuse the same USB connection after you close an Odin session, and you can't re-connect the device. You have to reboot each time.
2) Do not use the Linux version under WSL or under a badly configured VM. Do not expect any support on those - it's a waste of time.
3) Always try each option the platform-specific note tells you. In case of linux, try with cdc_asm disabled and enabled.

## Support me
Please consider donating [here](https://ko-fi.com/sussydev) if you would like to support me and other projects of sussy.dev (team of young developers making random projects for fun)

## List of platforms
1) [x] Linux (USB DevFS method)
2) [ ] Windows (not implemented)
3) [ ] Mac OS (not implemented)

## How's it different from Heimdall?
Fun fact: Official Odin for Linux works using DevFS, and people report it working when Heimdall didn't.
1) Is able to do NAND Erase All (aka erase userdata) and erase any partition, given the length.
2) Ability to shutdown and reboot into Odin mode (not supported on every device)
3) Not using a USB transfer library and doing it natively for best results
4) Extended the PIT parser with new information being discovered
5) Can flash directly from an Odin `.tar` / `.tar.md5` archive
6) Works with `.lz4` files directly without manual extraction
7) Implemented EFS Clear and Bootloader Update options in Odin
8) Is able to change the sales code of the device

## Current list of commands
![Commands](https://github.com/Samsung-Loki/Thor/assets/68467762/01526ec4-ff64-4308-8ff4-46af9e5aa0c1)

## Usage guide
<details>
  <summary>Initiate a connection</summary>
  <br>
  <img src="https://github.com/Samsung-Loki/Thor/assets/68467762/a98b9ff7-4346-4d64-9783-c22ba6a5a709"/>
</details>
<details>
  <summary>Select the options</summary>
  <br>
  <img src="https://github.com/Samsung-Loki/Thor/assets/68467762/6d93c907-d548-4637-a473-4a2eb7793dbc"/>
</details>
<details>
  <summary>Flash an odin archive</summary>
  <br>
  <video src="https://github.com/Samsung-Loki/Thor/assets/68467762/cb32aadb-c01c-474e-9d31-5fd6e704b846"/>
</details>
<details>
  <summary>Flash a single partition</summary>
  <br>
