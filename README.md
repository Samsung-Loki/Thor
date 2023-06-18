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

## What devices were tested?
Me personally tested only SM-G355H, which is old, but do not worry. \
The Odin protocol never introduced any major changes, so it should work.

## Frequently Asked Questions
1) A fatal error occurred. The required library *something* could not be found. \
This is a Linux Package Mixup, use [this](https://github.com/Samsung-Loki/LegacyThor/issues/5) as reference.

2) What happened to original Thor (or Hreidmar) \
This is a rewrite of Thor written from scratch, so I could implement the native USB communication. \
Also, the old codebase was severely flawed in my opinion. I archived it [here](https://github.com/Samsung-Loki/LegacyThor/).

## Credits
[TheAirBlow](https://github.com/theairblow) for Thor Flash Utility
[Benjamin-Dobell](https://github.com/Benjamin-Dobell) for documenting much of the Odin protocol

## This project is licenced under
[Mozilla Public License Version 2.0](https://github.com/Samsung-Loki/Thor/blob/main/LICENCE)
