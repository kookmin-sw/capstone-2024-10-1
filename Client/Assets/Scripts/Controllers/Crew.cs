using UnityEngine;
using Data;
using Fusion;

public class Crew : Creature
{
    #region Field

    public CrewData CrewData => CreatureData as CrewData;
    public CrewStat CrewStat => (CrewStat)BaseStat;
    public CrewAnimController CrewAnimController => (CrewAnimController)BaseAnimController;
    public Inventory Inventory { get; protected set; }

    public UI_CrewIngame CrewIngameUI => IngameUI as UI_CrewIngame;

    public GameObject RightHand { get; protected set; }
    public GameObject LeftHand { get; protected set; }

    public AudioSource AudioSource { get; protected set; }
    #endregion

    protected override void Init()
    {
        base.Init();

        Managers.ObjectMng.Crews[NetworkObject.Id] = this;

        Inventory = gameObject.GetComponent<Inventory>();
        AudioSource = gameObject.GetComponent<AudioSource>();

        Head = Util.FindChild(gameObject, "head.x", true);
        RightHand = Util.FindChild(gameObject, "c_middle1.r", true);
        LeftHand = Util.FindChild(gameObject, "c_middle1.l", true);
    }

    public override void SetInfo(int templateID)
    {
        CreatureType = Define.CreatureType.Crew;

        base.SetInfo(templateID);

        Head.transform.localScale = Vector3.zero;

        if (IsFirstPersonView)
        {
            CreatureCamera = Managers.ResourceMng.Instantiate("Cameras/CreatureCamera", Util.FindChild(gameObject, "neck.x", true).transform).GetComponent<CreatureCamera>();
            CreatureCamera.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            CreatureCamera.SetInfo(this);
        }
        else
        {
            WatchingCamera = Managers.ResourceMng.Instantiate("Cameras/WatchingCamera", gameObject.transform).GetComponent<WatchingCamera>();
            WatchingCamera.enabled = true;
            WatchingCamera.Creature = this;
        }

        CrewStat.SetStat(CrewData);
        Inventory.SetInfo();

        IsSpawned = true;

        if (HasStateAuthority && Managers.SceneMng.CurrentScene.IsSceneType((int)Define.SceneType.GameScene | (int)Define.SceneType.ReadyScene))
        {
            StartCoroutine(Managers.SceneMng.CurrentScene.OnSceneLoaded());
        }
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        UpdateStamina();
        PlayEffectMusic();
        StopEffectMusic();
    }

    protected override void HandleInput()
    {
        base.HandleInput();

        if (CreatureState == Define.CreatureState.Damaged || CreatureState == Define.CreatureState.Dead || CreatureState == Define.CreatureState.Use)
            return;

        CheckInteractable(false);

        if (CreatureState == Define.CreatureState.Interact)
        {
            if (Input.GetKeyDown(KeyCode.F))
                CreatureState = Define.CreatureState.Idle;
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
            if (CheckInteractable(true))
                return;

        if (Input.GetKeyDown(KeyCode.G))
            if (Inventory.DropItem())
                return;

        if (Input.GetMouseButtonDown(0))
            if (CheckAndUseItem())
                return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            Inventory.ChangeItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            Inventory.ChangeItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            Inventory.ChangeItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            Inventory.ChangeItem(3);

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            Inventory.ChangeItem(Mathf.Clamp(Inventory.CurrentItemIdx - 1, 0, 3));

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
            Inventory.ChangeItem(Mathf.Clamp(Inventory.CurrentItemIdx + 1, 0, 3));

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (CreaturePose != Define.CreaturePose.Sit)
                CreaturePose = Define.CreaturePose.Sit;
            else
                CreaturePose = Define.CreaturePose.Stand;
            return;
        }

        if (Velocity == Vector3.zero)
            CreatureState = Define.CreatureState.Idle;
        else
        {
            CreatureState = Define.CreatureState.Move;

            if (Input.GetKey(KeyCode.LeftShift) && Direction.z > 0)
            {
                if (CreaturePose != Define.CreaturePose.Sit && CrewStat.IsRunnable)
                    CreaturePose = Define.CreaturePose.Run;
                if (!CrewStat.IsRunnable)
                    CreaturePose = Define.CreaturePose.Stand;
            }
            else
                if (CreaturePose == Define.CreaturePose.Run)
                CreaturePose = Define.CreaturePose.Stand;
        }

    }

    protected bool CheckAndUseItem()
    {
        if (!HasStateAuthority || CreatureState == Define.CreatureState.Dead || !IsSpawned)
            return false;

        if (Inventory.CurrentItem == null)
        {
            Debug.Log("No Item");
            return false;
        }

        return Inventory.CheckAndUseItem();
    }

    #region Update

    protected void UpdateStamina()
    {
        if (CreaturePose == Define.CreaturePose.Run && CreatureState == Define.CreatureState.Move)
            CrewStat.OnStaminaChanged(-Define.RUN_USE_STAMINA * Runner.DeltaTime);
        else
            CrewStat.OnStaminaChanged(Define.PASIVE_RECOVER_STAMINA * Runner.DeltaTime);

        if (CreatureState == Define.CreatureState.Idle)
            CrewStat.OnSanityChanged(Define.PASIVE_RECOVER_SANITY * Runner.DeltaTime);
    }

    protected override void UpdateIdle()
    {
        switch (CreaturePose)
        {
            case Define.CreaturePose.Stand:
                break;
            case Define.CreaturePose.Sit:
                break;
            case Define.CreaturePose.Run:
                CreaturePose = Define.CreaturePose.Stand;
                break;
        }

        if (IsFirstPersonView)
        {
            KCC.SetLookRotation(0, CreatureCamera.transform.rotation.eulerAngles.y);
        }
    }

    protected override void UpdateMove()
    {
        switch (CreaturePose)
        {
            case Define.CreaturePose.Stand:
                BaseStat.Speed = CrewData.WalkSpeed;
                break;
            case Define.CreaturePose.Sit:
                BaseStat.Speed = CrewData.SitSpeed;
                break;
            case Define.CreaturePose.Run:
                BaseStat.Speed = CrewData.RunSpeed;
                break;
        }

        if (IsFirstPersonView)
        {
            KCC.SetLookRotation(0, CreatureCamera.transform.rotation.eulerAngles.y);
        }
        else
        {
            if (Velocity != Vector3.zero)
            {
                Quaternion newRotation = Quaternion.LookRotation(Velocity);
                KCC.SetLookRotation(newRotation);
            }
        }

        KCC.Move(Velocity, 0f);
    }

    protected override void UpdateInteract()
    {

    }

    protected override void UpdateUse()
    {

    }

    protected override void UpdateDead()
    {

    }

    #endregion

    #region Event

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_OnDamaged(int value)
    {
        CrewStat.OnHpChanged(-value);

        if (CrewStat.Hp <= 0)
        {
            OnDead();
            return;
        }

        CreatureState = Define.CreatureState.Damaged;
        CrewAnimController.PlayDamaged();
        ReturnToIdle(1f);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_OnSanityDamaged(int value)
    {
        CrewStat.OnSanityChanged(-value);
    }

    public void OnDead()
    {
        CreatureState = Define.CreatureState.Dead;

        CrewAnimController.PlayDead();
    }

    #endregion

    private void PlayEffectMusic()
    {
        if (CreatureState == Define.CreatureState.Move)
        {
            if (AudioSource.isPlaying == false)
            {
                AudioSource.volume = 0.5f;
                AudioSource.clip = Managers.SoundMng.GetOrAddAudioClip("Music/Clicks/Walk");
                AudioSource.Play();
            }
            else
            {
                if (CreaturePose == Define.CreaturePose.Stand)
                {
                    AudioSource.pitch = 1f;
                    AudioSource.volume = 0.5f;
                }
                if (CreaturePose == Define.CreaturePose.Sit)
                {
                    AudioSource.volume = 0.3f;
                    AudioSource.pitch = 1f;
                }
                if (CreaturePose == Define.CreaturePose.Run)
                {
                    AudioSource.pitch = 2f;
                    AudioSource.volume = 1f;
                }
                return;
            }
        }
    }
    private void StopEffectMusic()
    {
        if (CreatureState == Define.CreatureState.Idle)
        {
            AudioSource.Stop();
        }
    }

}
