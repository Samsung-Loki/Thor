![badges](https://img.shields.io/github/contributors/Samsung-Loki/Thor.svg)
![badges](https://img.shields.io/github/forks/Samsung-Loki/Thor.svg)
![badges](https://img.shields.io/github/stars/Samsung-Loki/Thor.svg)
![badges](https://img.shields.io/github/issues/Samsung-Loki/Thor.svg)
# Thor Flash Utility
This is a flash utility for Samsung devices, made from scratch in C#. \
To run this, you must have .NET 8 runtime installed on your computer. \
**This tool was tested and works flawlessly, I'm open for any bug reports.**

## Support me
If you would like to support me and my team, donate [here](https://ko-fi.com/sussydev). \
We're a team of teens (at age of 14-15) making random projects for fun.

## List of platforms
1) [x] Linux (USB DevFS method)
2) [ ] Windows (not implemented)
3) [ ] Mac OS (not implemented)

## How's it different from Heimdall?
Fun fact: Official Odin for Linux works using DevFS, and people report it working when Heimdall didn't.
1) Ability to shutdown and reboot into Odin mode (not supported on every device)
3) Not using a USB transfer library and doing it natively for best results
4) Extended the PIT parser with new information being discovered
5) Can flash directly from an Odin `.tar` / `.tar.md5` archive
6) Works with `.lz4` files directly without manual extraction
5) Implemented EFS Clear and Bootloader Update options in Odin

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
  <video src="https://github.com/Samsung-Loki/Thor/assets/68467762/f8f7e1dc-8c14-44c0-aaa7-85cf0c5cd024"/>
</details>
<details>
  <summary>Print description of a device's partition table</summary>
  <br>
  <video src="https://github.com/Samsung-Loki/Thor/assets/68467762/0e1a3335-71ea-45e7-bbcd-a61a553f4943"/>
</details>
<details>
  <summary>Dump device's partition table into a PIT file</summary>
  <br>
  <video src="https://github.com/Samsung-Loki/Thor/assets/68467762/ef6a5a67-c902-4af1-8de8-b5bbe4a3e9ef"/>
</details>
<details>
  <summary>Print description of any PIT file</summary>
  <br>
  <video src="https://github.com/Samsung-Loki/Thor/assets/68467762/4d4f3ccc-380a-4557-93fa-a2cc1ee698bc"/>
</details>

## What devices were tested?
1) SM-M205FN/DS from 2019 `0x00030000` (Unknown1: 0, Unknown2: 0, Version: 3)
2) SM-G355H/DS from 2014 `0x00020000` (Unknown1: 0, Unknown2: 0, Version: 2)

## Frequently Asked Questions
1) A fatal error occurred. The required library *something* could not be found. \
This is a Linux Package Mixup, use [this](https://github.com/Samsung-Loki/LegacyThor/issues/5) as reference.

2) What happened to original Thor (or Hreidmar) \
This is a rewrite of Thor written from scratch, so I could implement the native USB communication. \
Also, the old codebase was severely flawed in my opinion. I archived it [here](https://github.com/Samsung-Loki/LegacyThor/).

## Credits
[TheAirBlow](https://github.com/theairblow) for Thor Flash Utility \
[Benjamin-Dobell](https://github.com/Benjamin-Dobell) for documenting much of the Odin protocol

## This project is licenced under
[Mozilla Public License Version 2.0](https://github.com/Samsung-Loki/Thor/blob/main/LICENCE)
