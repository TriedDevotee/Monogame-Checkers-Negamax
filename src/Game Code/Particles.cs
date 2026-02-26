using Comp_Sci_NEA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

public class GameParticle
{
    Texture2D particleTexture;
    float acceleration;
    Vector2 velocity;
    Vector2 position;
    public float life;
    float maxLife;
    float angle;
    float opacity;
    float speed;
    float size;
    int screenWidth;
    int screenHeight;

    public GameParticle(Texture2D texture, 
                    float startX = 0.0f, 
                    float startY = 0.0f, 
                    float accMag = 2.0f, 
                    float velMag = 10.0f, 
                    float inputLife = 100.0f,  
                    float inputAngle = 45,
                    float startSize = 1.0f,
                    float startOpacity = 1.0f)
    {
        position = new Vector2(startX, startY);

        velocity = new Vector2(velMag, velMag);
        speed =  velMag;
        acceleration = accMag;

        life = inputLife;
        maxLife = inputLife;

        angle = inputAngle;

        screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        particleTexture = texture;
        //This is quite literally never used 

        size = startSize;
        opacity = startOpacity;
    }

    public void UpdateParticle(float dt)
    {
        speed += acceleration * dt;

        velocity.X = MathF.Cos(angle) * speed;
        velocity.Y = MathF.Sin(angle) * speed;

        position += velocity * dt ;

        life -= dt;

        if (isOOB())
        {
            life = 0;
        }

        size += 0.03f;

    }

    public bool isOOB()
    {
        return position.X < -50 || position.X > screenWidth + 50 || position.Y < -50 || position.Y > screenHeight + 50;
    }

    public void Draw(SpriteBatch sprites, Texture2D texture, Vector2 transformer)
    {

        if (life > maxLife * 0.9f) return;

        float alpha = opacity * (life / maxLife); 

        Color color;
        float layer = 0.8f;

        const float radConst = (float) Math.PI / 180.0f;

        color = new Color(
            (MathF.Cos(life) + 1f) * 0.5f, 
            (MathF.Cos(life + 120 * radConst) + 1f) * 0.5f, 
            (MathF.Cos(life + 240 * radConst) + 1f) * 0.5f);

        sprites.Draw(
            texture, 
            position + transformer,
            null, 
            color,
            angle, 
            new Vector2(texture.Width / 2f, texture.Height / 2f),  
            size, 
            SpriteEffects.None, 
            layer
        );

    }
}

public class GameBackground
{
    static readonly Random randint = new Random();
    List<GameParticle> particles;
    Vector2 centrePoint;
    Texture2D particleTexture;
    Vector2 transformer = new Vector2(0.0f, 0.0f);

    float screenWidth;
    float screenHeight;

    public float angle = 0.0f;

    public GameBackground(float xStart, float yStart, Texture2D inputTexture, float width, float height)
    {
        particles = new List<GameParticle>();
        centrePoint = new Vector2(xStart, yStart);
        particleTexture = inputTexture;

        screenWidth = width;
        screenHeight = height;
        

        frameZeroSetup();
    }

    public void AddParticles()
    {

        GameParticle newParticle = new GameParticle(
            texture: particleTexture,
            startX: centrePoint.X,
            startY: centrePoint.Y,
            accMag: 2.0f,
            velMag: 10.0f,
            inputLife: 30.0f,
            inputAngle: angle,
            startSize: 10.0f,
            startOpacity: 0.5f
        );

        angle += 0.2f;


        particles.Add(newParticle);

        centrePoint += transformer;
    }

    public void frameZeroSetup()
    {
        for (int i = 0; i < 2000; i++)
        {
            AddParticles();
            updateParticles(1f / 60f);
        }   
    }

    public void updateParticles(float dt)
    {
        foreach (GameParticle particle in particles)
        {
            particle.UpdateParticle(dt);
        }

        particles.RemoveAll(p => p.life == 0.0f);

        UpdateTransformer();
    }

    public void drawParticles(SpriteBatch sprite, Texture2D texture)
    {
        foreach (GameParticle particle in particles)
        {
            particle.Draw(sprite, texture, transformer);
        }
    }

    public void UpdateTransformer()
    {
        if (centrePoint.X > screenWidth || centrePoint.X < 0.0f) transformer.X *= -1;
        if (centrePoint.Y > screenHeight || centrePoint.Y < 0.0f) transformer.Y *= -1;
    }
}

public class MenuParticle
{
    public Vector2 Position {get; private set;}
    int height;
    int width;
    readonly Texture2D particleTexture;
    Color ParticleColor;

    public MenuParticle(float startX, float startY, Texture2D _texture, Color color, int step = -3, int h = 50, int w = 50)
    {
        Position = new Vector2(startX, startY);
        particleTexture = _texture;

        height = h;
        width = w;

        ParticleColor = color;
    }

    public void UpdateParticle()
    {
        Position = new Vector2(Position.X - 3, Position.Y);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle ParticlePosition = new((int) Position.X, (int) Position.Y, width, height);
        spriteBatch.Draw(particleTexture, ParticlePosition, ParticleColor);
    }

    public void moveParticle(float x, float y)
    {
        Position = new Vector2(Position.X + x, Position.Y + y);
    }
}

public class MenuBackground
{
    Session session;
    List<MenuParticle> Particles;
    Texture2D ParticleTexture;
    int particleSize = 0;
    int numColumns = 0;
    int numParticlesPerCol = 5;

    public MenuBackground(Session inSession, Texture2D texture)
    {
        session = inSession;
        ParticleTexture = texture;
        Particles = PopulateParticles();
    }

    private List<MenuParticle> PopulateParticles()
    {
        List<MenuParticle> particles = [];

        Color useColor = Color.White;

        particleSize = (int) session.Height / numParticlesPerCol;

        numColumns = (int) (session.Width / particleSize) + 2;

        for (int y = 0; y < numParticlesPerCol; y++)
        {
            for (int x = 0; x < numColumns; x++)
            {
                MenuParticle particle = new(x * particleSize, y * particleSize, ParticleTexture, useColor, h: particleSize, w: particleSize);
                particles.Add(particle);

                if (useColor == Color.White)
                {
                    useColor = Color.Red;
                } 
                else if (useColor == Color.Red)
                {
                    useColor = Color.Yellow;
                }
                else if (useColor == Color.Yellow)
                {
                    useColor = Color.Green;
                }
                else if (useColor == Color.Green)
                {
                    useColor = Color.Blue;
                }
                else
                {
                    useColor = Color.White;
                }
            }

            if (useColor == Color.White)
            {
                useColor = Color.Red;
            } 
            else if (useColor == Color.Red)
            {
                useColor = Color.Yellow;
            }
            else if (useColor == Color.Yellow)
            {
                useColor = Color.Green;
            }
            else if (useColor == Color.Green)
            {
                useColor = Color.Blue;
            }
            else
            {
                useColor = Color.White;
            }

        }

        return particles;
    }

    public void update(float dt)
    {
        float speed = 100f;

        foreach (MenuParticle particle in Particles)
        {
            particle.moveParticle(-speed * dt, 0);

            if (particle.Position.X <= -particleSize)
            {
                particle.moveParticle(particleSize * (numColumns), 0);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (MenuParticle particle in Particles)
        {
            particle.Draw(spriteBatch);
        }
    }
}
