# Hexagon
Download the built apk file from [here](https://github.com/musapkahraman/Hexagon/raw/master/HexagonMusapKahraman/Builds/HexagonMusapKahraman.apk).

## About

A game where the player tries to rotate hexagonal tiles in order to
create certain patterns. The game already exists, designed by Alexey Pajitnov who is
also known as the creator of Tetris.

## Requirements

- The core mechanic should be provided: Select a hexagonal group and turn them
clockwise or counterclockwise. If a 3-hexagonal group of the same color occurs, they
should explode, if not the hexagonal pieces should return to the initial state.
- The game grid should be changeable from the editor easily. The default grid is 8x9.
- Colors and color count of the hexagons should also be changeable from the editor
easily. By default, there are 5 colors.
- Scoring is 5 times the exploded block count.
- A bomb hexagon should appear on every 1000 score. The bomb’s function is to
count from a number every time an action is made on the board. When the number
reaches zero, a bomb explodes and the game is lost. You can check the example
game given to understand this requirement better.
- The game is over when there are no more available moves left or a bomb explodes.

## License
[MIT](https://choosealicense.com/licenses/mit/)
