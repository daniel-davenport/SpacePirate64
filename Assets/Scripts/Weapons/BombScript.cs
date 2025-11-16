using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static ShopScript;

public class BombScript : MonoBehaviour
{
    [Header("References")]
    public GameObject explosionRef;
    public GameObject bombRef;
    public GameObject blankRef;
    public WeaponHandler weaponHandler;
    public PlayerController playerController;
    public LevelDirector levelDirector;
    public SpawnDirector spawnDirector;
    public ScoreHandler scoreHandler;
    public EnemyPlane enemyPlane;

    [Header("Stats")]
    public string bombDisplayName;
    public string equippedBomb;
    public float bombCooldown; // default is 1 second
    private bool bombDebounce;
    public int bombDamage; // default is 5
    public int heldBombs; // default is 1
    public int maxBombs; // default is 3 - absolute max amount of bombs should be 4 (thats all that fits on the hud)

    public int bombDistance;

    private bool bombDetonated = false;

    // bomb data json
    [System.Serializable]
    public class Bomb
    {
        public string name;
        public string displayName;
        public int maxBombs;
    }

    [System.Serializable]
    public class BombList
    {
        public List<Bomb> bombs;
    }

    [SerializeField]

    // bomb list
    public BombList allBombsList = new BombList();
    private TextAsset bombListJson;
    private string lastBombType;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // setting default bomb type
        equippedBomb = "normal";
        bombDebounce = false;

        // loading projectiles
        bombRef = Resources.Load<GameObject>("Projectiles/BombProjectile");
        explosionRef = Resources.Load<GameObject>("Projectiles/ExplosionModel");
        blankRef = Resources.Load<GameObject>("Projectiles/BlankSphere");

        // loading refs
        scoreHandler = GetComponent<ScoreHandler>();

        // loading .json
        // reading bomb info from the physical json instead of a compiled one
        string bombFilePath = Path.Combine(Application.streamingAssetsPath, "BombData.json");
        if (File.Exists(bombFilePath))
        {
            string fileContent = File.ReadAllText(bombFilePath);
            bombListJson = new TextAsset(fileContent);

            allBombsList = JsonUtility.FromJson<BombList>(bombListJson.text);
        }
        else
        {
            print("ERROR: ITEM DATA FILE NOT FOUND.");
        }


        UpdateBombDisplayName();
    }

    // Update is called once per frame
    void Update()
    {
        // updating the display name of the bomb
        UpdateBombDisplayName();
    }

    // fires a bomb based on what you have equipped. tracks your held bombs and cooldowns.
    public void FireBomb()
    {
        if (bombDebounce == false && heldBombs > 0)
        {
            bombDetonated = false;
            bombDebounce = true;
            heldBombs -= 1;

            StartCoroutine(BombSwitch(equippedBomb));

            StartCoroutine(ResetBomb(bombCooldown));

            // some score for bombing
            scoreHandler.ChangePlayerScore("bombed");


        }
        else if (heldBombs <= 0)
        {
            print("out of bombs!");
        }

    }

    // resets your bomb cooldown.
    private IEnumerator ResetBomb(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        bombDebounce = false;
    }


    // make the bomb projectile
    // when the bomb projectile is destroyed, then do the bombswitch stuff?
    private IEnumerator CreateBomb(float fuseTime)
    {
        // instantiate the bomb ref
        GameObject bomb = Instantiate(bombRef, transform.position, Quaternion.identity);
        //bomb.transform.position = transform.position;

        // launcing it forward
        bomb.transform.DOMoveZ(bomb.transform.position.z + bombDistance, fuseTime).SetEase(Ease.OutQuad);
        //print(bomb.transform.position + " | " + transform.position);

        yield return new WaitForSeconds(fuseTime);

        bombDetonated = true;

        // destroy the bomb ref, create the explosion
        GameObject explosion = Instantiate(explosionRef);
        Renderer objectRenderer = explosion.GetComponent<Renderer>();

        explosion.transform.position = bomb.transform.position;

        Vector3 finalSize = explosion.transform.localScale * 8;

        // making it grow, fade out, and rotate
        explosion.transform.DOScale(finalSize, 1f).SetEase(Ease.OutExpo);
        explosion.transform.DORotate(new Vector3(0, 180, 0), 2f).SetEase(Ease.OutQuad);

        // fade out
        Color currentColor = objectRenderer.material.color;
        objectRenderer.material.DOColor(new Color(currentColor.r, currentColor.g, currentColor.b, 0), 1f).SetEase(Ease.OutExpo);

        Destroy(bomb);
        Destroy(explosion, 3f);

    }


    // updating the display name whenever the name changes
    private void UpdateBombDisplayName()
    {
        if (lastBombType != equippedBomb)
        {
            lastBombType = equippedBomb;
            int index = FindBombIndex(equippedBomb);

            //print(index);

            if (index >= 0)
            {
                string displayName = allBombsList.bombs[index].displayName;

                if (displayName != null)
                    bombDisplayName = displayName;

            }

        }
    }

    // get the index of the bomb in the json
    private int FindBombIndex(string bombName)
    {
        int index = -1;

        for (int i = 0; i < allBombsList.bombs.Count; i++)
        {
            if (allBombsList.bombs[i].name == bombName)
            {
                index = i;
                break;
            }
        }

        return index;
    }

    // bomb code is handled here
    // normally i'd have a dedicated script for every bomb type but this is fine for now since there aren't many bomb types.
    // IEnumerator so you can wait for the bombs to do stuff if you want
    private IEnumerator BombSwitch(string bombType)
    {
        switch (bombType)
        {

            // normal bomb
            // launched forward then explodes after a short delay
            // deals moderate damage to all enemies on screen.
            case "normal":
                //print("firing normal bomb");

                StartCoroutine(CreateBomb(1f));

                // wait for the bomb to detonate
                yield return new WaitUntil(() => bombDetonated == true);

                // get all enemies in the enemyplane then deal damage to all of them equal to your bomb's damage
                EnemyInit[] allEnemies = enemyPlane.gameObject.GetComponentsInChildren<EnemyInit>();

                foreach(EnemyInit enemyScript in allEnemies)
                {
                    // deal damage
                    enemyScript.TakeDamage(bombDamage);
                }


                break;


            // blank
            // fires instantly, banishing all projectiles.
            // deals no damage to enemies, moreso a panic button.
            case "blank":
                //print("firing blank");

                // destroy all projectiles on screen (use spawn director for this)
                spawnDirector.DestroyAllProjectiles();


                // make a sphere centered on the player, make it grow and fade out
                GameObject explosion = Instantiate(blankRef, transform.position, Quaternion.identity, transform);
                Renderer objectRenderer = explosion.GetComponent<Renderer>();

                float bigScale = 8000;
                Vector3 finalSize = new Vector3(bigScale, bigScale, bigScale);
                explosion.transform.localScale = Vector3.zero;

                // making it grow, fade out, and rotate
                explosion.transform.DOScale(finalSize, 0.8f).SetEase(Ease.OutExpo).SetLink(explosion);

                // fade out
                Color currentColor = objectRenderer.material.color;
                objectRenderer.material.DOColor(new Color(currentColor.r, currentColor.g, currentColor.b, 0), 0.85f).SetEase(Ease.OutExpo).SetLink(explosion);

                Destroy(explosion, 0.7f);



                break;




            // default case
            // nothing equipped, does nothing.
            default:
                print("no bomb equipped!");

                break;

        }


        yield break;
    }


}
