using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 유니티에 존재하는 씬매니저를 래핑한 매니저
/// </summary>
public class SceneManagerEx
{
    /// <summary>
    /// 현재 위치한 씬이 어디인지 알아낸다.
    /// 씬마다 베이스 씬을 상속한 스크립트를 부착한 @Scene 객체를 가지고 있다.
    /// </summary>
    public BaseScene CurrentScene
    {
        get { return GameObject.FindObjectOfType<BaseScene>(); }
    }

    public Define.Scene PreviousScene { get; set; }

    /// <summary>
    /// 씬을 불러온다. 다만 기존의 스트링이 아닌 enum 타입으로 가져올 수 있다.
    /// 내부적으로 메인 매니저의 씬을 초기화시키는 코드를 실행해 씬이 바뀔 때,
    /// 자동으로 초괴화 작업이 이루어지도록 한다.
    /// </summary>
    public void LoadScene(Define.Scene type)
    {
        Managers.Clear();
        PreviousScene = CurrentScene.SceneType;
        SceneManager.LoadScene(GetSceneName(type));
    }

    public void ReturnScene()
    {
        SceneManager.LoadScene(GetSceneName(PreviousScene));
        PreviousScene = CurrentScene.SceneType;
    }

    /// <summary>
    /// 씬의 이름을 가져온다. 씬의 이름에 특정 규칙이 있을 경우 여기에 반영한다.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    /// <summary>
    /// 베이스 씬을 상속한 각각의 씬에 정의되어 있는 클리어 함수를 불러온다.
    /// </summary>
    public void Clear()
    {
        CurrentScene.Clear();
    }
}
