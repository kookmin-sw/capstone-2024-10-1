using Fusion;
using System.Collections;
using UnityEngine;

public class BasicAttack : BaseSkill
{
    public override bool CheckAndUseSkill()
    {
        if (!Ready)
            return false;

        if (SkillData.Range > 0f)
            Owner.CurrentSkillRange = SkillData.Range;

        UseSkill();
        return true;
    }

    public override void UseSkill()
    {
        base.UseSkill();

        Owner.AlienAnimController.PlayBasicAttack();
        Rpc_PlayEffectMusic();
        StartCoroutine(ProgressSkill());
    }

    protected override IEnumerator ProgressSkill()
    {
        while (CurrentSkillAmount < SkillData.TotalSkillAmount)
        {
            Vector3 attackPosition = Owner.transform.position + ForwardDirection * SkillData.Range;
            Collider[] hitColliders = new Collider[3];

            if (!IsHit && Physics.OverlapSphereNonAlloc(attackPosition, SkillData.Range, hitColliders, LayerMask.GetMask("Crew")) > 0)
            {
                if (hitColliders[0].gameObject.TryGetComponent(out Crew crew))
                {
                    IsHit = true;
                    crew.Rpc_OnDamaged(SkillData.Damage);
                    crew.Rpc_OnSanityDamaged(SkillData.SanityDamage);
                }
            }

            UpdateWorkAmount(Time.deltaTime);
            yield return null;
        }

        SkillInterrupt();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_PlayEffectMusic()
    {
        Owner.AudioSource_1.volume = 1f;
        Owner.AudioSource_1.pitch = 1f;
        Owner.AudioSource_1.clip = Managers.SoundMng.GetOrAddAudioClip("Music/Clicks/Monster_Attack1");
        Owner.AudioSource_1.Play();
    }
}
