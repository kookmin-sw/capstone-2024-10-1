using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

//이 스크립트가 튜토리얼 종료까지 총괄
public class TutorialPlanSystem : NetworkBehaviour
{
    private int _batteryChargeCount;
    public int BatteryChargeCount
    {
        get => _batteryChargeCount;
        set
        {
            _batteryChargeCount = value;
            OnBatteryCharge();
        }
    }
    public bool IsBatteryChargeFinished { get; private set; }

    private bool _isCardKeyUsed;
    public bool IsCardKeyUsed
    {
        get => _isCardKeyUsed;
        set
        {
            _isCardKeyUsed = value;
            OnCardkeyUsed();
        }
    }

    private bool _isCentralComputerUsed;
    public bool IsCentralComputerUsed
    {
        get => _isCentralComputerUsed;
        set
        {
            _isCentralComputerUsed = value;
            OnCentralComputerUsed();
        }
    }

    private bool _isCargoGateOpen;
    public bool IsCargoGateOpen
    {
        get => _isCargoGateOpen;
        set
        {
            _isCargoGateOpen = value;
            OnCargoGateComputerUsed();
        }
    }

    public GameObject[] BatteryCharger;
    public GameObject[] CentralContolComputer;
    public GameObject[] CargoPassageContolComputer;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        Managers.TutorialMng.TutorialPlanSystem = this;

        BatteryCharger = GameObject.FindGameObjectsWithTag("BatteryCharger");
        CentralContolComputer = GameObject.FindGameObjectsWithTag("CentralControlComputer");
        CargoPassageContolComputer = GameObject.FindGameObjectsWithTag("CargoPassageControlComputer");

        if (Managers.ObjectMng.MyCreature is Alien) return;

        BatteryCharger.SetLayerRecursive(LayerMask.NameToLayer("InteractableObject"));
    }

    private void OnBatteryCharge()
    {
        if (Managers.ObjectMng.MyCreature is Alien) return;

        GameObject.FindWithTag("Player").GetComponent<TutorialCrew>()
            .CrewTutorialUI.TutorialPlanUI.GetComponent<UI_TutorialPlan>().UpdateBatteryCount(BatteryChargeCount);

        if (BatteryChargeCount == Define.TUTORIAL_BATTERY_CHARGE_GOAL)
        {
            IsBatteryChargeFinished = true;
            if (Managers.ObjectMng.MyCreature is Crew)
            {
                BatteryCharger.SetLayerRecursive(LayerMask.NameToLayer("MapObject"));
                CentralContolComputer.SetLayerRecursive(LayerMask.NameToLayer("InteractableObject"));
            }
        }
    }

    private void OnCardkeyUsed()
    {
        if (Managers.ObjectMng.MyCreature is Alien) return;

        var ui = Managers.ObjectMng.MyCrew.CrewIngameUI as UI_CrewTutorial;
        ui.TutorialPlanUI.GetComponent<UI_TutorialPlan>().OnCardkeyUsed();
    }

    private void OnCentralComputerUsed()
    {
        if (Managers.ObjectMng.MyCreature is Alien) return;

        var ui = Managers.ObjectMng.MyCrew.CrewIngameUI as UI_CrewTutorial;
        ui.TutorialPlanUI.GetComponent<UI_TutorialPlan>().OnCentralComputerUsed();

        CentralContolComputer.SetLayerRecursive(LayerMask.NameToLayer("MapObject"));
        CargoPassageContolComputer.SetLayerRecursive(LayerMask.NameToLayer("InteractableObject"));
    }

    private void OnCargoGateComputerUsed()
    {
        if (Managers.ObjectMng.MyCreature is Alien) return;

        var ui = Managers.ObjectMng.MyCrew.CrewIngameUI as UI_CrewTutorial;
        ui.TutorialPlanUI.GetComponent<UI_TutorialPlan>().OnCargoGateComputerUsed();
    }

    public void EndTutorial()
    {
        Managers.SceneMng.LoadScene(0);
    }
}
