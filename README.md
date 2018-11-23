This Tetris clone has been realized by Jorge Matas & Sinhuè Angelo Rossi using C# and Unity3D.

This implementation aims to be clean and CPU-friendly, so raycasts and physical elements are not being used. Geometrical transformations and a matrix of ints are instead used to manage the game grid.

The most interesting part is the grid management because it happens on two levels:
- the graphical one (pieces positions, transformations, etc.);
- the "actual" one (ints matrix that specifies the presence or not of a cube on each position).

There's also a trivial user interface that doesn't respect the best usability rules for sure even if it does what it is meant to do. It is there because allowed us to learn about UI on Unity3D, and we're sure that in a future project there will be more attention on it.

If you want to play, you can [download the latest release](https://github.com/pingumen96/tetrisclone/releases) or simply [run the online WebGL version](https://pingumen96.github.io/tetrisclone).
