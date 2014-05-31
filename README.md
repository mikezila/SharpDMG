# SharpDMG


A simple GameBoy emulator, with a twist, in C#.

### SharpDMG?
The internal codename for the GameBoy was "Dot Matrix Game".  DMG.  C-Sharp.  SharpDMG.

### What's the twist?
This is a simple GameBoy emulator being written with the goal of running real GameBoy cartridges using an Arduino microcontroller and a serial bridge.  The speed of this likely will not be great, but there are already tons of fast and flawless GameBoy emulators.  I can't think of any others that run real, physical cartridges.

It's being written in pure C# and Windows Forms.  It's being written using Visual Studio, but it should work just as well under Mono anywhere that Mono runs.  Using a real cartridge will require an Arduino loaded up with the sketch/driver from the repo.  Once that is hooked up to a cartridge connector with a game loaded, SharpDMG will connect to it over serial and the magic will happen.  In theory.  

You can also use standard rom files.  There will be facilities to dump games from cartridges as well.  Until emulation with rom files is working, hardware cartridge support won't be added.

### What is the point of this?
Learning.  Writing a GameBoy emulator is a reasonablly but not terribly complex project that is great as an exercise.  I also wanted to finally use all of the electronics/Arduino kits and parts I had around for something cool.  Wiring up a cartridge interface is pretty simple.  (Most) Arduinos run on 5V, which is what GameBoy cartridges run at, and using an Arduino Mega you have more than enough pins.  From then it's writing a pretty simple Arduino sketch to read bytes from the cartridge and send them over serial as they are requested.

The Arduino side is already written and ready.  The emulator itself is the main focus at the moment.