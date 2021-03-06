﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public const float MAXTIME = 120f;
    public float largeTimer = 10f;
    public float ballAverageSpawnTimer = 2f;
    public bool TrainingMode = true;
    public bool ballHasBeenTakenNonTraining;
    public bool powerUpBall = false;
    public GameObject ballPrefab;
    public GameObject dodgerPrefab;
    public GameObject powerUpSpawnLocation;
    public GameObject powerUpPrefab;
    public List<GameObject> powerUpList;
    public List<Dodger> dodgersList;

    private const float POWERUP_SPAWNTIMER = 15f;
    private float episodeTime = MAXTIME;
    private float ballRespawnTime;
    private float currentScoreDodger = 0f;
    private float currentScoreThrower = 3.6f;
    private float currentUpgradeTimer = POWERUP_SPAWNTIMER;
    private float largeScale = 2f;
    private float totalScoreOfDestroyedDodgers;
    private float highscore = 0f;
    private bool throwing = true;
    private bool spawnDodgers;
    private bool spawningPowerups = true;
    private System.Random random = new System.Random();
    private GameObject ballSpawnpointNonTraining;
    private GameObject balls;
    private GameObject dodgers;
    private Vector3 standardPositionDL;
    private Vector3 standardPositionDM;
    private Vector3 standardPositionDR;
    private TextMeshPro[] scoreboards;
    private TextMeshPro scoreboardDodgers;
    private TextMeshPro scoreboardThrower;
    private BoxCollider powerUpSpawnBox;

    // Start is called before the first frame update
    void Start()
    {
        if (!TrainingMode)
        {
            ballHasBeenTakenNonTraining = true;
            ballSpawnpointNonTraining = transform.Find("BallSpawnPoint").gameObject;
        }
        balls = transform.Find("Balls").gameObject;
        dodgersList = new List<Dodger>();
        ballRespawnTime = ballAverageSpawnTimer;
        StartCoroutine(PowerUpSpawner());
        StartCoroutine(BallSpawner()); //For training AI. Environment will throw ball instead of player.
        standardPositionDL = new Vector3(10f, 3f, -20f);
        standardPositionDM = new Vector3(0f, 3f, -20f);
        standardPositionDR = new Vector3(-10f, 3f, -20f);
        SpawnDodgersGameobject();
        SpawnDodgers();
        scoreboards = transform.GetComponentsInChildren<TextMeshPro>();
        for(int counter = 0; counter < scoreboards.Length; counter++)
        {
            if(scoreboards[counter].tag == "DodgerScoreboard")
            {
                scoreboardDodgers = scoreboards[counter];
            }
            else if(scoreboards[counter].tag == "ThrowerScoreboard")
            {
                scoreboardThrower = scoreboards[counter];
            }
        }
        powerUpSpawnBox = powerUpSpawnLocation.GetComponent<BoxCollider>();
    }
    void Update()
    {
        //Check if throwing is enabled
        if (throwing == false)
        {
            StartCoroutine(BallSpawner());
            throwing = true;
        }
        //Check if spawning power ups is enabled
        if (spawningPowerups == false && powerUpList.Count() < 3)
        {
            StartCoroutine(PowerUpSpawner());
            spawningPowerups = true;
        }
        //Check if powerup is enabled
        if (powerUpBall)
        {
            largeTimer -= Time.deltaTime;
            if (largeTimer <= 0)
            {
                powerUpBall = false;
            }
        }
        //Check if dodgers need respawn
        if (spawnDodgers)
        {
            SpawnDodgers();
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        currentScoreDodger = totalScoreOfDestroyedDodgers;
        
        if(episodeTime >= 0)
        {
            //Reset environment if dodgers are dead
            if (dodgersList.Count == 0)
            {
                ResetEnvironment();
            }
            else
            {
                for (int counter = dodgersList.Count - 1; counter >= 0; counter--)
                {
                    if (!dodgersList[counter].isHit)
                    {
                        //Get score from dodgers which are still alive
                        currentScoreDodger += dodgersList[counter].GetCumulativeReward();
                    }
                    else if (dodgersList[counter].isHit)
                    {
                        //End episode for dodger which has been hit
                        totalScoreOfDestroyedDodgers += dodgersList[counter].GetCumulativeReward();
                        dodgersList[counter].EndEpisode();
                        Destroy(dodgersList[counter].gameObject);
                        dodgersList.Remove(dodgersList[counter]);
                    }
                }
                //Show score and episode timer
                scoreboardDodgers.text = currentScoreDodger.ToString("f3") + "\n" + episodeTime.ToString("f0");
                scoreboardThrower.text = $"Record: {highscore.ToString("f0")} \n \nTime: {(MAXTIME - episodeTime).ToString("f0")}";
            }
            episodeTime = episodeTime - Time.deltaTime;
        } else
        {
            //End all episodes en reset environment if episoder timer = 0
            EndAllEpisodes();
            ResetEnvironment();
        }
    }
    //Reset Dodgerlist for environment
    private void SpawnDodgersGameobject()
    {
        dodgers = new GameObject();
        dodgers.name = "Dodgers";
        dodgers.transform.SetParent(this.transform);
        dodgers.transform.position = this.transform.position;
    }

    public void ResetEnvironment()
    {
        //Clear Powerup list
        foreach (GameObject powerUp in powerUpList)
        {
            Destroy(powerUp);
        }
        //Clear Ball list
        foreach (Transform ball in balls.transform)
        {
            Destroy(ball.gameObject);
        }
        //Destroy Dodgerlist and create new
        Destroy(dodgers.gameObject);
        SpawnDodgersGameobject();
        //Coroutine restarts
        StopAllCoroutines();
        //Reset Timers
        currentUpgradeTimer = POWERUP_SPAWNTIMER;
        //SpawnCheck
        spawnDodgers = true;
        throwing = false;
        spawningPowerups = false;
        ballHasBeenTakenNonTraining = true;
        totalScoreOfDestroyedDodgers = 0f;
        currentScoreThrower = 3.6f;
        if(highscore == 0f || MAXTIME - episodeTime < highscore) 
        {
            highscore = MAXTIME - episodeTime;
        }
        episodeTime = MAXTIME;
    }
    public void SpawnDodgers() 
    {
        //Set left dodger and spawn
        GameObject dodgerLeft = Instantiate(dodgerPrefab, transform);
        dodgerLeft.transform.SetParent(dodgers.transform);
        dodgerLeft.transform.localPosition = standardPositionDL;
        dodgerLeft.name = "dodgerLeft";
        //Set middle dodger and spawn
        GameObject dodgerMiddle = Instantiate(dodgerPrefab, transform);
        dodgerMiddle.transform.SetParent(dodgers.transform);
        dodgerMiddle.transform.localPosition = standardPositionDM;
        dodgerMiddle.name = "dodgerMiddle";
        //Set right dodger and spawn
        GameObject dodgerRight = Instantiate(dodgerPrefab, transform);
        dodgerRight.transform.SetParent(dodgers.transform);
        dodgerRight.transform.localPosition = standardPositionDR;
        dodgerRight.name = "dodgerRight";
        //Set dodgerlist and set spawn on false
        dodgersList = transform.GetComponentsInChildren<Dodger>(dodgers).ToList();
        spawnDodgers = false;
    }

    IEnumerator BallSpawner()
    {
        while (TrainingMode)
        {
            yield return new WaitForSeconds(ballRespawnTime);
            ballRespawnTime = Random.Range(ballAverageSpawnTimer * 0.5f, ballAverageSpawnTimer * 1.5f); //Random interval between throws
            GameObject ball = Instantiate(ballPrefab);

            //Spawn ball
            if (powerUpBall == true){
                ball.transform.localScale = new Vector3(largeScale, largeScale, largeScale);
            }
            ball.transform.SetParent(balls.transform);

            //Random position
            float ballX = Random.Range(transform.position.x -1f, transform.position.x + 1f);
            float ballY = Random.Range(transform.position.y + 0.8f,transform.position.y + 3f);
            float ballZ = transform.position.z + 18f;
            Vector3 ballPositie = new Vector3(ballX, ballY, ballZ);
            ball.transform.position = ballPositie;

            //Random (throw) velocity
            Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
            //select position of a dodger
            int randomDodgerNumber = random.Next(0, dodgersList.Count());
            GameObject randomDodger = dodgersList[randomDodgerNumber].gameObject;
            Vector3 randomDodgerPosition = (randomDodger.transform.position - ball.transform.position).normalized;
            //Throw ball
            Vector3 throwVector = new Vector3(randomDodgerPosition.x, randomDodgerPosition.y + 0.1f, randomDodgerPosition.z);
            ballRigidbody.velocity = throwVector * 60f;
        }
        while (!TrainingMode)
        {
            if (ballHasBeenTakenNonTraining)
            { 
                SpawnBall();
            }
            yield return new WaitForSeconds(3f);
        }
        throwing = false;
    }

    private void SpawnBall()
    {
        GameObject ball = Instantiate(ballPrefab);

        if (powerUpBall == true)
        {
            ball.transform.localScale = new Vector3(largeScale, largeScale, largeScale);
        }
        ball.transform.SetParent(balls.transform);
        ball.transform.position = ballSpawnpointNonTraining.transform.localPosition;
        ballHasBeenTakenNonTraining = false;
    }

    IEnumerator PowerUpSpawner()
    {
        while (powerUpList.Count() < 3)
        {
            yield return new WaitForSeconds(currentUpgradeTimer);
            //Instantiate powerup and add to list
            GameObject largePowerUp = Instantiate(powerUpPrefab);
            largePowerUp.GetComponent<EnlargeBallScript>().MyEnvironment = this;
            powerUpList.Add(largePowerUp);
            largePowerUp.transform.SetParent(GameObject.FindGameObjectWithTag("PowerUpList").transform);
            //The bounds for the spawn of the power ups
            float powerUpX = Random.Range(powerUpSpawnBox.bounds.min.x, powerUpSpawnBox.bounds.max.x);
            float powerUpY = Random.Range(0.3f, powerUpSpawnBox.bounds.max.y);
            float powerUpZ = Random.Range(powerUpSpawnBox.bounds.min.z, powerUpSpawnBox.bounds.max.z);
            largePowerUp.transform.position = new Vector3(powerUpX, powerUpY, powerUpZ);        
        }
        spawningPowerups = false;
    }
    
    private void EndAllEpisodes()
    {
        if(dodgersList.Count > 0)
        {
            for (int counter = dodgersList.Count - 1; counter >= 0; counter--)
            {
                dodgersList[counter].EndEpisode();
                if(dodgersList[counter] != null && dodgersList[counter].gameObject != null)
                {
                    Destroy(dodgersList[counter].gameObject);
                }
                dodgersList.RemoveAt(counter);
            }
        }
    }
}
