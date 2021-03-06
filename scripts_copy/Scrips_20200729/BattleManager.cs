﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.SceneManagement;
namespace PCRCaculator.Battle
{
    public enum eGameBattleState { PREPARING = 0,FIGHTING=1,WIN=2,LOSE=3,TIME_UP=4 }
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance;
        private static BattleUIManager uIManager;
        public Transform spineParent;
        private List<UnitCtrl> playersList = new List<UnitCtrl>();
        private List<UnitCtrl> enemiesList = new List<UnitCtrl>();
        private List<UnitCtrl> blackOutUnitList = new List<UnitCtrl>();

        private eGameBattleState gameState;
        private List<UnitData> playerData;
        private List<UnitData> enemyData;
        private float battleTime = 0;
        private float frameRate = 60;
        private bool isPause;
        public eGameBattleState GameState { get => gameState;}
        public List<UnitCtrl> PlayersList { get => playersList;}
        public List<UnitCtrl> EnemiesList { get => enemiesList;}
        public List<UnitCtrl> BlackOutUnitList { get => blackOutUnitList; set => blackOutUnitList = value; }
        public float BattleTime { get => battleTime; set => battleTime = value; }
        public float FrameRate { get => frameRate; set => frameRate = value; }
        public bool IsPause { get => isPause; set => isPause = value; }
        public float DeltaTimeForPause { get => IsPause ? 0 :Time.deltaTime; }

        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            gameState = eGameBattleState.PREPARING;
            uIManager = BattleUIManager.Instance;
            MainManager.Instance.GetBattleData(out playerData,out enemyData);
            CreateSpines(playerData, enemyData);
            uIManager.SetUI();
            gameState = eGameBattleState.FIGHTING;
        }
        private void Update()
        {
            if (isPause) { return; }
            BattleTime += Time.deltaTime;
        }
        public void Pause()
        {
            foreach(UnitCtrl a in PlayersList)
            {
                a.Pause();
            }
            foreach(UnitCtrl b in EnemiesList)
            {
                b.Pause();
            }
            IsPause = !IsPause;
        }
        public int Random(int min,int max)
        {
            if (min >= max) { return min; }
            return UnityEngine.Random.Range(min, max);
        }
        public void ExitButton()
        {
            SceneManager.LoadScene("BeginScene");
        }
        /// <summary>
        /// 创建敌我双方的战斗小人
        /// </summary>
        /// <param name="players">我方，已经排好顺序</param>
        /// <param name="enemies">敌方，已经排好顺序</param>
        private void CreateSpines(List<UnitData> players, List<UnitData> enemies)
        {
            int i = 0;
            int j = 0;
            foreach(UnitData a in players)
            {
                CreateSpine(a,i,false);
                i++;
            }
            foreach(UnitData b in enemies)
            {
                CreateSpine(b, j, true);
                j++;
            }
        }
        private void CreateSpine(UnitData unitData,int posid,bool isother)
        {
            int unitid = unitData.unitId;
            SkeletonDataAsset dataAsset = ScriptableObject.CreateInstance<SkeletonDataAsset>();
            dataAsset = Resources.Load<SkeletonDataAsset>("Unit/" + unitid + "/" + unitid + "_SkeletonData");
            var sa = SkeletonAnimation.NewSkeletonAnimationGameObject(dataAsset); // Spawn a new SkeletonAnimation GameObject.
            sa.gameObject.name = unitid.ToString();
            sa.Initialize(false);
            sa.transform.SetParent(spineParent);
            UnitCtrl ctrl = sa.gameObject.AddComponent<UnitCtrl>();
            if (!isother)
            {
                playersList.Add(ctrl);
            }
            else
            {
                enemiesList.Add(ctrl);
            }
            BattleUnitBaseSpineController unitActionController = sa.gameObject.AddComponent<BattleUnitBaseSpineController>();

            unitActionController.SetOnAwake(dataAsset, unitid);
            ctrl.SetOnAwake(unitActionController,unitData,isother);
            ctrl.SetPosition(new Vector3Int(560 + 200 * posid, posid,0));
            ctrl.SetState(eActionState.GAMESTART,0);
            
        }
    }
}