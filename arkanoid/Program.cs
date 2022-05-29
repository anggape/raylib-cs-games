using System.Numerics;
using Ape.RaylibGames.Arkanoid;
using Raylib_cs;
using static Raylib_cs.Raylib;

const int PLAYER_MAX_LIFE = 5;
const int LINES_OF_BRICKS = 5;
const int BRICKS_PER_LINE = 20;

const int screenWidth = 800;
const int screenHeight = 450;
bool gameOver = false;
bool pause = false;
var player = new Player();
var ball = new Ball();
var brick = new Brick[LINES_OF_BRICKS][];
var brickSize = Vector2.Zero;

for (int i = 0; i < brick.GetLength(0); i++)
    brick[i] = new Brick[BRICKS_PER_LINE];

InitWindow(screenWidth, screenHeight, "classic game: arkanoid");
InitGame();
SetTargetFPS(60);

while (!WindowShouldClose())
    UpdateDrawFrame();

UnloadGame();
CloseWindow();

void InitGame()
{
    brickSize = new Vector2(GetScreenWidth() / BRICKS_PER_LINE, 40);

    // Initialize player
    player.Position = new Vector2(screenWidth / 2, screenHeight * 7 / 8);
    player.Size = new Vector2(screenWidth / 10, 20);
    player.Life = PLAYER_MAX_LIFE;

    // Initialize ball
    ball.Position = new Vector2(screenWidth / 2, screenHeight * 7 / 8 - 30);
    ball.Speed = new Vector2(0, 0);
    ball.Radius = 7;
    ball.Active = false;

    // Initialize bricks
    int initialDownPosition = 50;

    for (int i = 0; i < LINES_OF_BRICKS; i++)
    {
        for (int j = 0; j < BRICKS_PER_LINE; j++)
        {
            brick[i][j].Position = new Vector2(
                j * brickSize.X + brickSize.X / 2,
                i * brickSize.Y + initialDownPosition
            );
            brick[i][j].Active = true;
        }
    }
}

void UpdateGame()
{
    if (!gameOver)
    {
        if (IsKeyPressed(KeyboardKey.KEY_P))
            pause = !pause;

        if (!pause)
        {
            // Player movement logic
            if (IsKeyDown(KeyboardKey.KEY_LEFT))
                player.Position.X -= 5;
            if ((player.Position.X - player.Size.X / 2) <= 0)
                player.Position.X = player.Size.X / 2;
            if (IsKeyDown(KeyboardKey.KEY_RIGHT))
                player.Position.X += 5;
            if ((player.Position.X + player.Size.X / 2) >= screenWidth)
                player.Position.X = screenWidth - player.Size.X / 2;

            // Ball launching logic
            if (!ball.Active)
            {
                if (IsKeyPressed(KeyboardKey.KEY_SPACE))
                {
                    ball.Active = true;
                    ball.Speed = new Vector2(0, -5);
                }
            }

            // Ball movement logic
            if (ball.Active)
            {
                ball.Position.X += ball.Speed.X;
                ball.Position.Y += ball.Speed.Y;
            }
            else
            {
                ball.Position = new Vector2(player.Position.X, screenHeight * 7 / 8 - 30);
            }

            // Collision logic: ball vs walls
            if (
                ((ball.Position.X + ball.Radius) >= screenWidth)
                || ((ball.Position.X - ball.Radius) <= 0)
            )
                ball.Speed.X *= -1;
            if ((ball.Position.Y - ball.Radius) <= 0)
                ball.Speed.Y *= -1;
            if ((ball.Position.Y + ball.Radius) >= screenHeight)
            {
                ball.Speed = new Vector2(0, 0);
                ball.Active = false;

                player.Life--;
            }

            // Collision logic: ball vs player
            if (
                CheckCollisionCircleRec(
                    ball.Position,
                    ball.Radius,
                    new Rectangle(
                        player.Position.X - player.Size.X / 2,
                        player.Position.Y - player.Size.Y / 2,
                        player.Size.X,
                        player.Size.Y
                    )
                )
            )
            {
                if (ball.Speed.Y > 0)
                {
                    ball.Speed.Y *= -1;
                    ball.Speed.X = (ball.Position.X - player.Position.X) / (player.Size.X / 2) * 5;
                }
            }

            // Collision logic: ball vs bricks
            for (int i = 0; i < LINES_OF_BRICKS; i++)
            {
                for (int j = 0; j < BRICKS_PER_LINE; j++)
                {
                    if (brick[i][j].Active)
                    {
                        // Hit below
                        if (
                            (
                                (ball.Position.Y - ball.Radius)
                                <= (brick[i][j].Position.Y + brickSize.Y / 2)
                            )
                            && (
                                (ball.Position.Y - ball.Radius)
                                > (brick[i][j].Position.Y + brickSize.Y / 2 + ball.Speed.Y)
                            )
                            && (
                                (MathF.Abs(ball.Position.X - brick[i][j].Position.X))
                                < (brickSize.X / 2 + ball.Radius * 2 / 3)
                            )
                            && (ball.Speed.Y < 0)
                        )
                        {
                            brick[i][j].Active = false;
                            ball.Speed.Y *= -1;
                        }
                        // Hit above
                        else if (
                            (
                                (ball.Position.Y + ball.Radius)
                                >= (brick[i][j].Position.Y - brickSize.Y / 2)
                            )
                            && (
                                (ball.Position.Y + ball.Radius)
                                < (brick[i][j].Position.Y - brickSize.Y / 2 + ball.Speed.Y)
                            )
                            && (
                                (MathF.Abs(ball.Position.X - brick[i][j].Position.X))
                                < (brickSize.X / 2 + ball.Radius * 2 / 3)
                            )
                            && (ball.Speed.Y > 0)
                        )
                        {
                            brick[i][j].Active = false;
                            ball.Speed.Y *= -1;
                        }
                        // Hit left
                        else if (
                            (
                                (ball.Position.X + ball.Radius)
                                >= (brick[i][j].Position.X - brickSize.X / 2)
                            )
                            && (
                                (ball.Position.X + ball.Radius)
                                < (brick[i][j].Position.X - brickSize.X / 2 + ball.Speed.X)
                            )
                            && (
                                (MathF.Abs(ball.Position.Y - brick[i][j].Position.Y))
                                < (brickSize.Y / 2 + ball.Radius * 2 / 3)
                            )
                            && (ball.Speed.X > 0)
                        )
                        {
                            brick[i][j].Active = false;
                            ball.Speed.X *= -1;
                        }
                        // Hit right
                        else if (
                            (
                                (ball.Position.X - ball.Radius)
                                <= (brick[i][j].Position.X + brickSize.X / 2)
                            )
                            && (
                                (ball.Position.X - ball.Radius)
                                > (brick[i][j].Position.X + brickSize.X / 2 + ball.Speed.X)
                            )
                            && (
                                (MathF.Abs(ball.Position.Y - brick[i][j].Position.Y))
                                < (brickSize.Y / 2 + ball.Radius * 2 / 3)
                            )
                            && (ball.Speed.X < 0)
                        )
                        {
                            brick[i][j].Active = false;
                            ball.Speed.X *= -1;
                        }
                    }
                }
            }

            // Game over logic
            if (player.Life <= 0)
                gameOver = true;
            else
            {
                gameOver = true;

                for (int i = 0; i < LINES_OF_BRICKS; i++)
                {
                    for (int j = 0; j < BRICKS_PER_LINE; j++)
                    {
                        if (brick[i][j].Active)
                            gameOver = false;
                    }
                }
            }
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
        // Draw player bar
        DrawRectangle(
            (int)(player.Position.X - player.Size.X / 2),
            (int)(player.Position.Y - player.Size.Y / 2),
            (int)player.Size.X,
            (int)player.Size.Y,
            Color.BLACK
        );

        // Draw player lives
        for (int i = 0; i < player.Life; i++)
            DrawRectangle(20 + 40 * i, screenHeight - 30, 35, 10, Color.LIGHTGRAY);

        // Draw ball
        DrawCircleV(ball.Position, ball.Radius, Color.MAROON);

        // Draw bricks
        for (int i = 0; i < LINES_OF_BRICKS; i++)
        {
            for (int j = 0; j < BRICKS_PER_LINE; j++)
            {
                if (brick[i][j].Active)
                {
                    if ((i + j) % 2 == 0)
                        DrawRectangle(
                            (int)(brick[i][j].Position.X - brickSize.X / 2),
                            (int)(brick[i][j].Position.Y - brickSize.Y / 2),
                            (int)brickSize.X,
                            (int)brickSize.Y,
                            Color.GRAY
                        );
                    else
                        DrawRectangle(
                            (int)(brick[i][j].Position.X - brickSize.X / 2),
                            (int)(brick[i][j].Position.Y - brickSize.Y / 2),
                            (int)brickSize.X,
                            (int)brickSize.Y,
                            Color.DARKGRAY
                        );
                }
            }
        }

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

void UnloadGame() { }

void UpdateDrawFrame()
{
    UpdateGame();
    DrawGame();
}
