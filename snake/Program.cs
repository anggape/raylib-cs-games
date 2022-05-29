using System.Numerics;
using Ape.RaylibGames.Snake;
using Raylib_cs;
using static Raylib_cs.Raylib;

const int SNAKE_LENGTH = 256;
const int SQUARE_SIZE = 31;

//------------------------------------------------------------------------------------
// Global Variables Declaration
//------------------------------------------------------------------------------------
const int screenWidth = 800;
const int screenHeight = 450;
int framesCounter = 0;
bool gameOver = false;
bool pause = false;
var fruit = new Food();
var snake = new Snake[SNAKE_LENGTH];
var snakePosition = new Vector2[SNAKE_LENGTH];
var allowMove = false;
var offset = Vector2.Zero;
var counterTail = 0;

// Initialization (Note windowTitle is unused on Android)
//---------------------------------------------------------
InitWindow(screenWidth, screenHeight, "classic game: snake");

InitGame();

SetTargetFPS(60);

//--------------------------------------------------------------------------------------

// Main game loop
while (!WindowShouldClose()) // Detect window close button or ESC key
{
    // Update and Draw
    //----------------------------------------------------------------------------------
    UpdateDrawFrame();
    //----------------------------------------------------------------------------------
}

// De-Initialization
//--------------------------------------------------------------------------------------
UnloadGame(); // Unload loaded data (textures, sounds, models...)

CloseWindow(); // Close window and OpenGL context

//--------------------------------------------------------------------------------------


//------------------------------------------------------------------------------------
// Module Functions Definitions (local)
//------------------------------------------------------------------------------------

// Initialize game variables
void InitGame()
{
    framesCounter = 0;
    gameOver = false;
    pause = false;

    counterTail = 1;
    allowMove = false;

    offset.X = screenWidth % SQUARE_SIZE;
    offset.Y = screenHeight % SQUARE_SIZE;

    for (int i = 0; i < SNAKE_LENGTH; i++)
    {
        snake[i].position = new Vector2(offset.X / 2, offset.Y / 2);
        snake[i].size = new Vector2(SQUARE_SIZE, SQUARE_SIZE);
        snake[i].speed = new Vector2(SQUARE_SIZE, 0);

        if (i == 0)
            snake[i].color = Color.DARKBLUE;
        else
            snake[i].color = Color.BLUE;
    }

    for (int i = 0; i < SNAKE_LENGTH; i++)
    {
        snakePosition[i] = new Vector2(0.0f, 0.0f);
    }

    fruit.Size = new Vector2(SQUARE_SIZE, SQUARE_SIZE);
    fruit.Color = Color.SKYBLUE;
    fruit.Active = false;
}

// Update game (one frame)
void UpdateGame()
{
    if (!gameOver)
    {
        if (IsKeyPressed(KeyboardKey.KEY_P))
            pause = !pause;

        if (!pause)
        {
            // Player control
            if (IsKeyPressed(KeyboardKey.KEY_RIGHT) && (snake[0].speed.X == 0) && allowMove)
            {
                snake[0].speed = new Vector2(SQUARE_SIZE, 0);
                allowMove = false;
            }
            if (IsKeyPressed(KeyboardKey.KEY_LEFT) && (snake[0].speed.X == 0) && allowMove)
            {
                snake[0].speed = new Vector2(-SQUARE_SIZE, 0);
                allowMove = false;
            }
            if (IsKeyPressed(KeyboardKey.KEY_UP) && (snake[0].speed.Y == 0) && allowMove)
            {
                snake[0].speed = new Vector2(0, -SQUARE_SIZE);
                allowMove = false;
            }
            if (IsKeyPressed(KeyboardKey.KEY_DOWN) && (snake[0].speed.Y == 0) && allowMove)
            {
                snake[0].speed = new Vector2(0, SQUARE_SIZE);
                allowMove = false;
            }

            // Snake movement
            for (int i = 0; i < counterTail; i++)
                snakePosition[i] = snake[i].position;

            if ((framesCounter % 5) == 0)
            {
                for (int i = 0; i < counterTail; i++)
                {
                    if (i == 0)
                    {
                        snake[0].position.X += snake[0].speed.X;
                        snake[0].position.Y += snake[0].speed.Y;
                        allowMove = true;
                    }
                    else
                        snake[i].position = snakePosition[i - 1];
                }
            }

            // Wall behaviour
            if (
                ((snake[0].position.X) > (screenWidth - offset.X))
                || ((snake[0].position.Y) > (screenHeight - offset.Y))
                || (snake[0].position.X < 0)
                || (snake[0].position.Y < 0)
            )
            {
                gameOver = true;
            }

            // Collision with yourself
            for (int i = 1; i < counterTail; i++)
            {
                if (
                    (snake[0].position.X == snake[i].position.X)
                    && (snake[0].position.Y == snake[i].position.Y)
                )
                    gameOver = true;
            }

            // Fruit position calculation
            if (!fruit.Active)
            {
                fruit.Active = true;
                fruit.Position = new Vector2(
                    GetRandomValue(0, (screenWidth / SQUARE_SIZE) - 1) * SQUARE_SIZE + offset.X / 2,
                    GetRandomValue(0, (screenHeight / SQUARE_SIZE) - 1) * SQUARE_SIZE + offset.Y / 2
                );

                for (int i = 0; i < counterTail; i++)
                {
                    while (
                        (fruit.Position.X == snake[i].position.X)
                        && (fruit.Position.Y == snake[i].position.Y)
                    )
                    {
                        fruit.Position = new Vector2(
                            GetRandomValue(0, (screenWidth / SQUARE_SIZE) - 1) * SQUARE_SIZE
                                + offset.X / 2,
                            GetRandomValue(0, (screenHeight / SQUARE_SIZE) - 1) * SQUARE_SIZE
                                + offset.Y / 2
                        );
                        i = 0;
                    }
                }
            }

            // Collision
            if (
                (
                    snake[0].position.X < (fruit.Position.X + fruit.Size.X)
                    && (snake[0].position.X + snake[0].size.X) > fruit.Position.X
                )
                && (
                    snake[0].position.Y < (fruit.Position.Y + fruit.Size.Y)
                    && (snake[0].position.Y + snake[0].size.Y) > fruit.Position.Y
                )
            )
            {
                snake[counterTail].position = snakePosition[counterTail - 1];
                counterTail += 1;
                fruit.Active = false;
            }

            framesCounter++;
        }
    }
    else
    {
        if (IsKeyPressed(KeyboardKey.KEY_ENTER))
        {
            InitGame();
            gameOver = false;
        }
    }
}

// Draw game (one frame)
void DrawGame()
{
    BeginDrawing();

    ClearBackground(Color.RAYWHITE);

    if (!gameOver)
    {
        // Draw grid lines
        for (int i = 0; i < screenWidth / SQUARE_SIZE + 1; i++)
        {
            DrawLineV(
                new Vector2(SQUARE_SIZE * i + offset.X / 2, offset.Y / 2),
                new Vector2(SQUARE_SIZE * i + offset.X / 2, screenHeight - offset.Y / 2),
                Color.LIGHTGRAY
            );
        }

        for (int i = 0; i < screenHeight / SQUARE_SIZE + 1; i++)
        {
            DrawLineV(
                new Vector2(offset.X / 2, SQUARE_SIZE * i + offset.Y / 2),
                new Vector2(screenWidth - offset.X / 2, SQUARE_SIZE * i + offset.Y / 2),
                Color.LIGHTGRAY
            );
        }

        // Draw snake
        for (int i = 0; i < counterTail; i++)
            DrawRectangleV(snake[i].position, snake[i].size, snake[i].color);

        // Draw fruit to pick
        DrawRectangleV(fruit.Position, fruit.Size, fruit.Color);

        if (pause)
            DrawText(
                "GAME PAUSED",
                screenWidth / 2 - MeasureText("GAME PAUSED", 40) / 2,
                screenHeight / 2 - 40,
                40,
                Color.GRAY
            );
    }
    else
        DrawText(
            "PRESS [ENTER] TO PLAY AGAIN",
            GetScreenWidth() / 2 - MeasureText("PRESS [ENTER] TO PLAY AGAIN", 20) / 2,
            GetScreenHeight() / 2 - 50,
            20,
            Color.GRAY
        );

    EndDrawing();
}

// Unload game variables
void UnloadGame()
{
    // TODO: Unload all dynamic loaded data (textures, sounds, models...)
}

// Update and Draw (one frame)
void UpdateDrawFrame()
{
    UpdateGame();
    DrawGame();
}
