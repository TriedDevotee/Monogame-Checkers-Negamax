using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
public class Particle
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

    public Particle(Texture2D texture, 
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

        size = startSize;
        opacity = startOpacity;
    }

    public void updateParticle(float dt)
    {
        //Do velocity update
        speed += acceleration * dt;

        velocity.X = MathF.Cos(angle) * speed;
        velocity.Y = MathF.Sin(angle) * speed;

        //Do position update
        position += velocity * dt ;

        //Update life
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

        color = new Color((MathF.Cos(life) + 1f) * 0.5f, (MathF.Cos(life + 2.094f) + 1f) * 0.5f, (MathF.Cos(life + 4.188f) + 1f) * 0.5f) * alpha ;

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

public class Background
{
    static readonly Random randint = new Random();
    List<Particle> particles;
    Vector2 centrePoint;
    Texture2D particleTexture;
    Vector2 transformer = new Vector2(0.0f, 0.0f);

    float screenWidth;
    float screenHeight;

    public float angle = 0.0f;

    public Background(float xStart, float yStart, Texture2D inputTexture, float width, float height)
    {
        particles = new List<Particle>();
        centrePoint = new Vector2(xStart, yStart);
        particleTexture = inputTexture;

        screenWidth = width;
        screenHeight = height;
        

        frameZeroSetup();
    }

    public void AddParticles()
    {

        Particle newParticle = new Particle(
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
        foreach (Particle particle in particles)
        {
            particle.updateParticle(dt);
        }

        particles.RemoveAll(p => p.life == 0.0f);

        UpdateTransformer();
    }

    public void drawParticles(SpriteBatch sprite, Texture2D texture)
    {
        foreach (Particle particle in particles)
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
