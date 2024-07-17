using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;

public class BattleSystem : MonoBehaviour
{

    [SerializeField] private enum BattleState { Start, Selection, Battle, Won, Lost, Run }

    [Header("Battle State")]
    [SerializeField] private BattleState state;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] partySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Battlers")]
    [SerializeField] private List<BattleEntities> allBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> enemyBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> playerBattlers = new List<BattleEntities>();

    [Header("UI")]
    [SerializeField] private GameObject[] enemySelectionButtons;
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private GameObject enemySelectionMenu;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private GameObject bottomTextPopUp;
    [SerializeField] private TextMeshProUGUI bottomText;



    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private int currentPlayer;

    private const string ACTION_MESSAGE = "'s Action:";
    private const string WIN_MESSAGE = "Your Party won the battle!";
    private const int TURN_DURATION = 2;


    void Start()
    {
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        enemyManager = GameObject.FindFirstObjectByType<EnemyManager>();

        CreatePartyEntities();
        CreateEnemyEntities();

        ShowBattleMenu();

    }

    private IEnumerator BattleRoutine()
    {
        //enemy selection menu disabled
        enemySelectionMenu.SetActive(false);

        //change state to the battle state
        state = BattleState.Battle;

        //enable our bottom text
        bottomTextPopUp.SetActive(true);

        //loop though all our battlers
        //-> do their approrpiate action

        for (int i = 0; i < allBattlers.Count; i++)
        {
            switch (allBattlers[i].BattleAction)
            {
                case BattleEntities.Action.Attack:
                    //do attack
                    yield return StartCoroutine(AttackRoutine(i));
                    break;
                case BattleEntities.Action.Run:
                    //run
                    break;
                default:
                    UnityEngine.Debug.Log("Error - incorrect battle action");
                    break;
            }
        }

        if (state == BattleState.Battle)
        {
            bottomTextPopUp.SetActive(false);
            currentPlayer = 0;
            ShowBattleMenu();

        }

        //if we havent won or lost, repeat the loop by opening the battle menu
        yield return null;

    }

    private IEnumerator AttackRoutine(int i)
    {
        //players turn
        if (allBattlers[i].IsPlayer == true)
        {
            BattleEntities currAttacker = allBattlers[i];
            BattleEntities currTarget = allBattlers[currAttacker.Target];

            //attack selected enemey (attack action)
            AttackAction(currAttacker, currTarget);
            
            //wait a few seconds
            yield return new WaitForSeconds(TURN_DURATION);

            //kill the enemy
            if(currTarget.CurrHealth <=0)
            {
                bottomText.text = string.Format("{0} deafeted {1}", currAttacker.Name, currTarget.Name);

                yield return new WaitForSeconds(TURN_DURATION);

                enemyBattlers.Remove(currTarget);
                allBattlers.Remove(currTarget);

                if(enemyBattlers.Count <= 0)
                {
                    state = BattleState.Won;
                    bottomText.text = WIN_MESSAGE;
                    yield return new WaitForSeconds(TURN_DURATION);
                    //go back to overworld
                }
            }

            //if no enemies remain
            //we won the battle

        }


        //enemies turn
        // attack the selected party member(attack action)
        //wait a few seconds
        //kill the party member

        //if no party members remain
        //battle lost  
    }

    private void CreatePartyEntities()
    {
        //get current party
        List<PartyMember> currentParty = new List<PartyMember>();
        currentParty = partyManager.GetCurrentParty();

        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentParty[i].MemberName, currentParty[i].CurrHealth, currentParty[i].MaxHealth,
            currentParty[i].Initiattive, currentParty[i].Strength, currentParty[i].Level, true);

            //spawning the visuals
            BattleVisuals tempBattleVisuals = Instantiate(currentParty[i].MemberBattleVisualPrefab,
            partySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            //set visual starting values
            tempBattleVisuals.SetStartingValues(currentParty[i].MaxHealth, currentParty[i].MaxHealth, currentParty[i].Level);
            //assign it to the battle entity
            tempEntity.BattleVisuals = tempBattleVisuals;


            allBattlers.Add(tempEntity);
            playerBattlers.Add(tempEntity);
        }

        //create battle entities for our party members
        //assign the values
    }

    private void CreateEnemyEntities()
    {
        //get current enemies
        List<Enemy> currentEnemies = new List<Enemy>();
        currentEnemies = enemyManager.GetCurrentEnemies();

        for (int i = 0; i < currentEnemies.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentEnemies[i].EnemyName, currentEnemies[i].CurrHealth, currentEnemies[i].MaxHealth,
            currentEnemies[i].Initiattive, currentEnemies[i].Strength, currentEnemies[i].Level, false);

            //spawning the visuals
            BattleVisuals tempBattleVisuals = Instantiate(currentEnemies[i].EnemyVisualPrefab,
            enemySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();

            //set visual starting values
            tempBattleVisuals.SetStartingValues(currentEnemies[i].MaxHealth, currentEnemies[i].MaxHealth, currentEnemies[i].Level);

            //assign it to the battle entity
            tempEntity.BattleVisuals = tempBattleVisuals;

            allBattlers.Add(tempEntity);
            enemyBattlers.Add(tempEntity);
        }

        //create battle entities for our party members
        //assign the values
    }

    public void ShowBattleMenu()
    {
        //who's action it is
        actionText.text = playerBattlers[currentPlayer].Name + ACTION_MESSAGE;

        //enabling our battle menu
        battleMenu.SetActive(true);

    }

    public void ShowEnemySelectionMenu()
    {
        //disable the battle menu
        battleMenu.SetActive(false);
        //set our enemy selection buttons
        SetEnemySelectionButtons();
        //enable our selection menu
        enemySelectionMenu.SetActive(true);



    }

    public void SetEnemySelectionButtons()
    {
        //disable all buttons
        for (int i = 0; i < enemySelectionButtons.Length; i++)
        {
            enemySelectionButtons[i].SetActive(false);
        }

        //enable buttons for each enemy (using count since its a list and not an array)
        for (int j = 0; j < enemyBattlers.Count; j++)
        {
            enemySelectionButtons[j].SetActive(true);
            enemySelectionButtons[j].GetComponentInChildren<TextMeshProUGUI>().text = enemyBattlers[j].Name;
        }
        //change the buttons text

    }

    public void SelectEnemy(int currentEnemy)
    {
        //setting the current members target
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];
        currentPlayerEntity.SetTarget(allBattlers.IndexOf(enemyBattlers[currentEnemy]));

        //tell the battle system this member intends to attack
        currentPlayerEntity.BattleAction = BattleEntities.Action.Attack;

        //increment through our party members
        currentPlayer++;

        //if all players have selected an action
        if (currentPlayer >= playerBattlers.Count)
        {
            //start battle
            StartCoroutine(BattleRoutine());

        }
        else
        {
            enemySelectionMenu.SetActive(false);
            ShowBattleMenu();
        }

    }

    private void AttackAction(BattleEntities currAttacker, BattleEntities currTarget)
    {
        //get damage
        int damage = currAttacker.Strength;
        //play the attack animation
        currAttacker.BattleVisuals.PlayAttackAnimation();
        //dealing the damage
        currTarget.CurrHealth -= damage;
        //play their hit animation
        currTarget.BattleVisuals.PlayHitAnimation();
        //update the UI
        currTarget.UpdateUI();
        bottomText.text = string.Format("{0} attacks {1} for {2} damage", currAttacker.Name, currTarget.Name, damage);

    }

}

[System.Serializable]
public class BattleEntities
{

    public enum Action { Attack, Run }
    public Action BattleAction;

    public string Name;
    public int CurrHealth;
    public int MaxHealth;
    public int Initiattive;
    public int Strength;
    public int Level;
    public BattleVisuals BattleVisuals;
    public bool IsPlayer;
    public int Target;

    public void SetEntityValues(string name, int currHealth, int maxHealth, int initiative, int strength, int level, bool isPlayer)
    {
        Name = name;
        CurrHealth = currHealth;
        MaxHealth = maxHealth;
        Initiattive = initiative;
        Strength = strength;
        Level = level;
        IsPlayer = isPlayer;
    }

    public void SetTarget(int target)
    {
        Target = target;
    }

    public void UpdateUI()
    {
        BattleVisuals.ChangeHealth(CurrHealth);
    }

}
