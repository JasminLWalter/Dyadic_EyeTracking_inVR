﻿using UnityEngine;
using UnityEngine.UI;

public enum CrosshairColorChannel
{
    RED,
    GREEN,
    BLUE,
    ALPHA
}

[System.Serializable]
public class Crosshair
{
    [Range(1, 150), Tooltip("Controls the length of each crosshair line.")]
    public int size = 10;

    [Range(1, 100), Tooltip("Controls the width of each crosshair line.")]
    public int thickness = 2;

    [Range(0, 350), Tooltip("Controls the distance between the center of the crosshair and the start of each crosshair line.")]
    public int gap = 5;

    [Tooltip("Specifies the color of the crosshair.")]
    public Color color = Color.green;

    public int SizeNeeded
    {
        private set { }
        get
        {
            int width = size + size + gap + gap;
            return width > thickness ? width : thickness;
        }
    }
}

public class SimpleCrosshair : MonoBehaviour
{
    [SerializeField]
private Material invisible;

    [SerializeField, Tooltip("Contains properties that Specify how the crosshair looks.")]
    private Crosshair m_crosshair = null;

    [Tooltip("Specifies the image to draw the crosshair to. If you leave this empty, this script generates a Canvas and an Image with the correct settings for you.")]
    public Image m_crosshairImage;

    [Tooltip("The game object that the crosshair should follow.")]
    public GameObject targetGameObject;

    private void Awake()
    {
        if(m_crosshairImage == null)
        {
            InitialiseCrosshairImage();
        }

        GenerateCrosshair();
    }

    private void Update()
    {
        if (targetGameObject != null)
        {
            // Update the crosshair position to match the target game object's position
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(targetGameObject.transform.position);
            m_crosshairImage.rectTransform.position = screenPosition;
        }
    }

    public void InitialiseCrosshairImage()
    {
        GameObject crosshairGameObject = new GameObject();
        crosshairGameObject.name = "Crosshair Canvas";

        Canvas crosshairCanvas = crosshairGameObject.AddComponent<Canvas>();
        crosshairCanvas.renderMode = RenderMode.WorldSpace;

        crosshairGameObject.AddComponent<CanvasScaler>();

        GameObject imageGameObject = new GameObject();
        imageGameObject.name = "Crosshair Image";
        imageGameObject.transform.parent = crosshairGameObject.transform;

        m_crosshairImage = imageGameObject.AddComponent<Image>();
        m_crosshairImage.rectTransform.localPosition = new Vector2(0, 0);
        m_crosshairImage.raycastTarget = false;

        // Set the size of the canvas to be large enough to accommodate the crosshair
        crosshairCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 500);

        // Position the canvas in front of the camera
        crosshairGameObject.transform.SetParent(Camera.main.transform);
        crosshairGameObject.transform.localPosition = new Vector3(0, 0, 2); // Adjust as needed
        crosshairGameObject.transform.localRotation = Quaternion.identity;
    }

    public void GenerateCrosshair()
    {
        Texture2D crosshairTexture = DrawCrosshair(m_crosshair);

        m_crosshairImage.rectTransform.sizeDelta = new Vector2(m_crosshair.SizeNeeded, m_crosshair.SizeNeeded);
        Sprite crosshairSprite = Sprite.Create(crosshairTexture,
            new Rect(0, 0, crosshairTexture.width, crosshairTexture.height),
            Vector2.one / 2);

        m_crosshairImage.sprite = crosshairSprite;
    }

    public void SetColor(CrosshairColorChannel channel, int value, bool redrawCrosshair)  // Set between 0 and 255
    {
        switch (channel)
        {
            case CrosshairColorChannel.RED:
                {
                    m_crosshair.color.r = value / 255.0f;
                }
                break;
            case CrosshairColorChannel.GREEN:
                {
                    m_crosshair.color.g = value / 255.0f; ;
                }
                break;
            case CrosshairColorChannel.BLUE:
                {
                    m_crosshair.color.b = value / 255.0f; ;
                }
                break;
            case CrosshairColorChannel.ALPHA:
                {
                    m_crosshair.color.a = value / 255.0f; ;
                }
                break;
        }
        if (redrawCrosshair)
        {
            GenerateCrosshair();
        }
    }

    public void SetColor(Color color, bool redrawCrosshair)
    {
        m_crosshair.color = color;
        if (redrawCrosshair)
        {
            GenerateCrosshair();
        }
    }

    public void SetThickness(int newThickness, bool redrawCrosshair)
    {
        m_crosshair.thickness = newThickness;
        if (m_crosshair.thickness < 1) { m_crosshair.thickness = 1; }
        if (redrawCrosshair)
        {
            GenerateCrosshair();
        }
    }

    public void SetSize(int newSize, bool redrawCrosshair)
    {
        m_crosshair.size = newSize;
        if(m_crosshair.size < 1) { m_crosshair.size = 1; }
        if (redrawCrosshair)
        {
            GenerateCrosshair();
        }
    }

    public void SetGap(int newGap, bool redrawCrosshair)
    {
        m_crosshair.gap = newGap;
        if (m_crosshair.gap < 0) { m_crosshair.gap = 0; }
        if (redrawCrosshair)
        {
            GenerateCrosshair();
        }
    }

    #region Getters
    public int GetSize() { return m_crosshair.size; }
    public int GetThickness() { return m_crosshair.thickness; }
    public int GetGap() { return m_crosshair.gap; }
    public Color GetColor() { return m_crosshair.color; }
    public Crosshair GetCrosshair() { return m_crosshair; }
    #endregion

    #region Draw Crosshair Texture
    public Texture2D DrawCrosshair(Crosshair crosshair = null)
    {
        if(crosshair == null) { crosshair = m_crosshair; }

        int sizeNeeded = crosshair.SizeNeeded;
        int centerBias = sizeNeeded / 2;

        Texture2D crosshairTexture = new Texture2D(sizeNeeded, sizeNeeded, TextureFormat.RGBA32, false);
        crosshairTexture.wrapMode = TextureWrapMode.Clamp;
        crosshairTexture.filterMode = FilterMode.Point;

        crosshairTexture.SetPixelData(new Color32[sizeNeeded * sizeNeeded], 0);

        // Top
        int startGapShort = Mathf.CeilToInt(crosshair.thickness / 2.0f);
        DrawBox(centerBias - startGapShort,
            centerBias + crosshair.gap,
            crosshair.thickness,
            crosshair.size,
            crosshairTexture,
            crosshair.color);

        // Right
        DrawBox(centerBias + crosshair.gap,
            centerBias - startGapShort,
            crosshair.size,
            crosshair.thickness,
            crosshairTexture,
            crosshair.color);

        // Bottom
        DrawBox(centerBias - startGapShort,
            centerBias - crosshair.gap - crosshair.size,
            crosshair.thickness,
            crosshair.size,
            crosshairTexture,
            crosshair.color);

        // Left
        DrawBox(centerBias - crosshair.gap - crosshair.size,
           centerBias - startGapShort,
           crosshair.size,
           crosshair.thickness,
           crosshairTexture,
           crosshair.color);

        crosshairTexture.Apply();
        return crosshairTexture;
    }

    private void DrawBox(int startX, int startY, int width, int height, Texture2D target, Color color)
    {
        if (startX + width > target.width ||
            startY + height > target.height)
        {
            Debug.LogWarning("Crosshair box is out of range.");
            return;
        }
        for (int x = startX; x < startX + width; ++x)
        {
            for (int y = startY; y < startY + height; ++y)
            {
                target.SetPixel(x, y, color);
            }
        }
    }
    //newColor;

    public Renderer rend; // Reference to the Renderer component

    // Function to assign a material
    public void AssignMaterial()
    {
        if (rend != null)
        {
            rend.material = invisible;
            Debug.Log("Material assigned: ");
        }
    }

public void MakeInvisible()
{
    if (m_crosshairImage != null)
    {
        Color newColor = m_crosshairImage.color;
        newColor.a = 0f; // Make the image fully transparent
        m_crosshairImage.color = newColor;
        Debug.Log("Crosshair image made invisible");
    }
}

public void MakeRed()
{
    if (m_crosshair != null)
    {
        // Update the color to red
        Color newColor = m_crosshairImage.color;
        newColor.a = 1f; // Make the image fully opaque
        m_crosshairImage.color = newColor;
        Debug.Log("Crosshair image made visible");
    }
}




    #endregion
}
