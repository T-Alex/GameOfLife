# Game Of Life
![Build](https://github.com/alex-titarenko/gameoflife/workflows/Build/badge.svg?branch=master)

WPF realization of Conway's Game of Life.

## Description
Interesting version of the computer realization of mathematical game "Life" invented by British mathematician John Conway in 1970. This game is the best known example of cellular automaton.

Venue of the game - "the universe" - is marked on the cell surface, unlimited, limited, or closed. Every cell on this surface can be in two states: dead or alive. The player does not take a direct part in the game, he only puts the initial configuration of live cells, which then interact according to certain rules without his participation. These rules lead to a huge variety of "life" that may arise in the game.

This implementation provides you "unlimited" size of the universe, it can scale, load and save in many popular file formats. Also there is the opportunity to set rules for the game, the generation time of the new generations, etc. The editor stores user actions that can, if necessary, roll them up to a certain moment, transformations such as rotations or flips can be applied on the selected areas. It is possible to work with the clipboard that allows you to copy and paste the samples when it's necessary. Also you can choose the color for the states of cells, gridlines, and background. The game has nice and intuitive user interface that will help to fill gameplay with new feelings.

## License
GameOfLife is under the [MIT license](LICENSE.md).
