# League of Legends Mastery Checker
Simple application that let you see your champion mastery points, it also has pop up window, which shows three closest champions to next mastery level.
It was originally designed for my girlfriend for streaming purposes as HUD for currently grinded champions, but it can be also used to look up your mastery points pretty quickly.
All points, levels, images are extracted using riot api with helper library [RiotSharp](https://github.com/BenFradet/RiotSharp).

## Compiling
This repository uses Net Framework 4.7.2, so to be able to compile it make sure you have this installed in your visual studio 2019.
Just launch .sln file and hit build, that's it!

## Usage
In order to use the API you need an API key which you can get [here](https://developer.riotgames.com).
After you get one, simply change "YOUR_API_KEY" string to your API key.

## Preview
##### Main Window
![main window](https://i.imgur.com/c5OUllA.png)
##### Top 3 Window
![top 3 window](https://i.imgur.com/dSXyARQ.png)
