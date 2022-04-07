using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using DG.Tweening.Core;

public class Tile : MonoBehaviour
{
    public static Tile selectedTile;

    [Header("Tile")]
    public Sprite[] tileTextures;
    public Color tileColor;

    [Header("Text")]
    public TMP_Text letterText;
    public TMP_Text scoreText;
    public Color textColor;

    [Header("Sprite References")]
    public SpriteRenderer tileSprite;
    public SpriteRenderer shadow;
    
    [Header("Collider References")]
    public Collider2D solidCollider;
    public Collider2D selectionCollider;


    [HideInInspector] public bool movable = true;
    [HideInInspector] public bool selected = false;
    [HideInInspector] public bool inTray = false;
    [HideInInspector] public bool placed = false;
    [HideInInspector] public TileSpace placedSpace;

    [HideInInspector] public char letter = ' ';
    [HideInInspector] public int score = 0;

    public TweenerCore<Vector3, Vector3, VectorOptions> currentScaleTween;
    public TweenerCore<Vector3, Vector3, VectorOptions> currentMoveTween;

    public bool IsVowel => letter == 'A' || letter == 'E' || letter == 'I' || letter == 'O' || letter == 'U';
    Rigidbody2D rb;
    TargetJoint2D targetJoint;
    SliderJoint2D sliderJoint;

    bool init = false;

    [HideInInspector] public Vector2 lastScreenPosition;

    public void Init(char _letter)
    {
        letter = _letter;
        gameObject.name = letter.ToString();
        init = true;
        score = TileBag.tileScores[letter];

        letterText.text = letter.ToString();
        scoreText.text = score.ToString();

        letterText.color = textColor;
        scoreText.color = textColor;
        scoreText.color = new Color(scoreText.color.r, scoreText.color.g, scoreText.color.b, 0.7f);
        tileSprite.color = tileColor;
        tileSprite.sprite = tileTextures[Random.Range(0, tileTextures.Length)];

        tileSprite.size = new Vector2(1, 1);

        if (rb == null) rb = gameObject.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
        if (targetJoint == null) targetJoint = gameObject.AddComponent(typeof(TargetJoint2D)) as TargetJoint2D;
        targetJoint.autoConfigureTarget = false;
        targetJoint.enabled = false;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.drag = 10;
        rb.useAutoMass = false;
        rb.mass = 1;

        if(sliderJoint == null) sliderJoint = gameObject.AddComponent(typeof(SliderJoint2D)) as SliderJoint2D;
        sliderJoint.useLimits = true;
        sliderJoint.connectedBody = TileTray.instance.rigidbody;
        sliderJoint.connectedAnchor = Vector2.zero;
        sliderJoint.autoConfigureAngle = false;
        sliderJoint.angle = 0;
        

        transform.localScale = Vector3.zero;

        lastScreenPosition = (Vector2)Camera.main.WorldToViewportPoint(transform.position);
    }

    Vector2 cameraDelta;
    Vector2 lastCamera;
    float lastCameraZoom;
        
    public void LateUpdate()
    {
        cameraDelta = (Vector2)Camera.main.transform.position - lastCamera;
        lastCamera = Camera.main.transform.position;

        bool zoomChanged = TileTray.instance.ScreenWidth != lastCameraZoom;
        lastCameraZoom = TileTray.instance.ScreenWidth;

        if((cameraDelta.magnitude > 0 || zoomChanged) && inTray)
        {
            transform.position = (Vector3)(Vector2)Camera.main.ViewportToWorldPoint(lastScreenPosition) + transform.position.z * Vector3.forward;
            transform.localScale = TileTray.instance.TargetUITileSize;
            TileTray.instance.UpdateTileLimits();
        }
        else
        {
            lastScreenPosition = (Vector2)Camera.main.WorldToViewportPoint(transform.position);
        }
    }

    public void Update()
    {
        if(!init) Debug.LogWarning("Tile must be initialized before use with Tile.Init()!");

        if((Input.touchCount == 0 || Input.GetMouseButtonUp(0)) && selected)
        {
            Deselect();
        }

        if(Input.GetMouseButtonDown(0) && Input.touchCount == 1){
            Vector2 distance = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position;
            if(distance.sqrMagnitude < 1.5f)
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, ~LayerMask.NameToLayer("Tiles"));
                if(hit.collider == selectionCollider)
                {
                    Select();
                }
            }
        }
    }

    public void FixedUpdate()
    {
        if (selected)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GetComponent<TargetJoint2D>().target = mousePos;
        }
    }

    public void Select()
    {
        if(!movable) return;

        if(inTray) RemoveFromTray();
        
        IncrementSortingOrder(10);
        selectedTile = this;
        selected = true;
        inTray = false;
        solidCollider.enabled = false;
        selectionCollider.enabled = false;
        targetJoint.enabled = true;
        rb.isKinematic = false;
        rb.drag = 0;

        Camera.main.GetComponent<PanAndZoom>().controlCamera = false;
        
        if(currentScaleTween != null) currentScaleTween.Kill();
        currentScaleTween = transform.DOScale(TileTray.instance.TargetUITileSize * 1.2f, 0.2f).SetEase(Ease.InOutSine);
        
        if(placed) UserPickupTile();
    }

    public void Deselect()
    {
        IncrementSortingOrder(-10);

        selectedTile = null;
        selected = false;
        rb.drag = 10;
        solidCollider.enabled = true;
        selectionCollider.enabled = true;
        targetJoint.enabled = false;

        Camera.main.GetComponent<PanAndZoom>().controlCamera = true;

        if (Board.instance.EmptySpaceSelected) UserPlaceTile(Board.instance.selectedSpace.coordinates);
        else MoveToTray();
    }

    public void MoveToTray(bool updateScale = true)
    {
        TileTray.instance.inTray.Add(this);
        inTray = true;
        solidCollider.enabled = true;
        

        currentMoveTween.Kill();
        var tween = transform.DOMoveY(TileTray.instance.rigidbody.position.y, 0.2f).SetEase(Ease.InOutSine);
        currentMoveTween = tween;
        tween.OnComplete(() =>
        {
            sliderJoint.enabled = true;
            if(updateScale) TileTray.instance.UpdateTileScale();
        });
    }

    public void RemoveFromTray(bool updateScale = true)
    {
        TileTray.instance.inTray.Remove(this);
        inTray = false;
        solidCollider.enabled = false;
        sliderJoint.enabled = false;

        if(updateScale) TileTray.instance.UpdateTileScale();
    }

    public void SetPlacedProperties()
    {
        solidCollider.enabled = false;
        IncrementSortingOrder(-10);
        shadow.enabled = false;
        placed = true;

        if(GetComponent<Rigidbody2D>())
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
        }
        if(currentScaleTween != null) currentScaleTween.Kill();
        currentScaleTween = transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.InOutSine);
    }

    public void SetPickupProperties()
    {
        solidCollider.enabled = false;
        IncrementSortingOrder(10);
        shadow.enabled = true;
        placed = false;

        if(GetComponent<Rigidbody2D>())
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = false;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
        }
    }

    public void UserPlaceTile(Vector2Int space)
    {
        SetPlacedProperties();

        Board.instance.userPlacedTiles.Add(this);
        Board.instance.SetSpace(space, this);
        placedSpace = Board.instance.spaces[space];
        Board.instance.totalTilesOnBoard++;

        currentMoveTween.Kill();
        var tween = transform.DOMove(Board.instance.spaces[space].transform.position + Vector3.forward * transform.position.z, 0.1f).SetEase(Ease.InOutSine);
        currentMoveTween = tween;
        tween.OnComplete(() =>
        {
            GameManager.instance.CheckBoard();
        });
    }

    public void UserPickupTile()
    {
        if(placedSpace == null) return;
        if(placedSpace.tile == null) return;
        if(!movable) return;

        SetPickupProperties();
        Board.instance.SetSpace(placedSpace.coordinates, null);
        placedSpace = null;
        Board.instance.totalTilesOnBoard--;
        Board.instance.userPlacedTiles.Remove(this);

        GameManager.instance.CheckBoard();
    }

    public void IncrementSortingOrder(int by)
    {
        letterText.GetComponent<MeshRenderer>().sortingOrder += by;
        scoreText.GetComponent<MeshRenderer>().sortingOrder += by;
        tileSprite.sortingOrder += by;
        if(tileSprite.GetComponentInChildren<SpriteMask>())
        { 
            tileSprite.GetComponentInChildren<SpriteMask>().frontSortingOrder += by;
            tileSprite.GetComponentInChildren<SpriteMask>().backSortingOrder += by;
        }
        shadow.sortingOrder += by;
    }
    
}