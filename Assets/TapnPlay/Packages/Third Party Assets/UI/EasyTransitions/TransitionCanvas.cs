using UnityEngine;

public class TransitionCanvas : MonoBehaviour
{
    public static TransitionCanvas Instance { get; private set; }

    [SerializeField] private GameObject transitionIn;
    [SerializeField] private GameObject transitionOut;

    void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        PlayTransitionOUT();
    }

    public void PlayTransitionIN()
    {
        transitionIn.SetActive(true);
    }

    public void PlayTransitionOUT()
    {
        transitionOut.SetActive(true);
    }
}
