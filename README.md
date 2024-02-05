# Partak

## Overview

Remake of the classic open source game 'Liquid Wars'.

This game was on the iOS and Google Play stores for about the last 8 hours. I am proud to say during it's time it maintained 4+ stars and had a small handful of users that seemed to enjoy it very much.

Unfortunately, the game was never profitable, nor really made any money at all. It is also a pain to keep apps updated and maintained to be published on iOS and Google App Stores. So I've decided to fully open source and unlicense it.

Also, this was initially written in such an old version of Unity that nothing of the core game logic would I implement the same today. This should be jobs based, or even compute shader based. 

However a bunch of the dependency management code, how I wire up all the UI prefabs and ServiceObjects through Addressables and my GTPooling library, is some newer work that I feel is still a good, and rather fancy, approach to dealing with this in Unity. I may expand on this and write some posts about this approach.

Watch Demo Video:
[![Watch Demo Video](https://img.youtube.com/vi/G2kN3NSHp7Y/maxresdefault.jpg)](https://youtu.be/G2kN3NSHp7Y)

## Usage

The project has submodules, so ensure to clone recursively. `git clone git@github.com:rygo6/Partak-Unity.git --recursive`

You should be able to open the project in Unity 2019.4.31f1, open the Partak/Core/Scenes/Main scene, click play, and have it work. 

You will need to manually create your own level to play as the AWS services to download the levels are shut down. 

Anything which relied on the AWS connection will simply not work, or error. Some alternate backend would need to be setup, or made to make that work again. I have no intention of doing this.

I did scrape the AWS server and download all of the user created levels before shutting the AWS services down. These levels are in `UserGeneratedLevels` folder. Unfortunately I've not implemented anything to deal with these levels so it is merely for archive purposes. 

## License

This is UNLICENSED. So do what you will. This is primarily up there to give some code examples to other googling for solutins that I may have solved in here.
