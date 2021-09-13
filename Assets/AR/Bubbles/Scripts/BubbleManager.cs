using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSourceLanguage))]
public class BubbleManager : MonoBehaviour
{
    [SerializeField] GameObject[] bubblesInOrder;
    [SerializeField] ArrayList bubblesPlayed;

    [SerializeField] AudioSourceLanguage FirstInstruction;
    [SerializeField] AudioSourceLanguage FirstBubbleTouched;
    [SerializeField] AudioSourceLanguage AllBubblesPlayedOnce;
    [SerializeField] AudioSourceLanguage NotRightOrder;
    [SerializeField] AudioSourceLanguage CelebrationRightOrder;

    SoundPlayerNothingHappening soundPlayerNothingHappening;

  
    [SerializeField] GameObject replayAudioButtonEN;
    [SerializeField] GameObject replayAudioButtonDE;

    [SerializeField] GameObject UIPlayCountContainer;
    [SerializeField] GameObject UIPlayCountPrefab;

    // this flag indicates if the user managed to touch a bubble
    private bool enableOnboarding = true;

    // this flag indicates if the user has played bubbles for X times
    private bool allBubblesPlayedForOnce = false;


    private void Awake()
    {
        soundPlayerNothingHappening = GetComponentInChildren<SoundPlayerNothingHappening>(true);
       
    }

    void Start()
    {
        EnableFirstBubble(false);

        

        // don't start everything at once, delay audio plaback for one second
        StartCoroutine(PlayAudioAndWait(FirstInstruction, ()=> {
            EnableFirstBubble();
        }, 1));
    }

    void OnEnable()
    {
        // for testing in edit mode
#if UNITY_EDITOR
        enableOnboarding = true;
        allBubblesPlayedForOnce = false;
        EnableFirstBubble(false);
      
        replayAudioButtonEN.SetActive(false);
        replayAudioButtonDE.SetActive(false);

        UIPlayCountContainer.SetActive(false);

#endif
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void AddBubblePlay(GameObject bubble)
    {
        if (enableOnboarding)
        {
            Log.d("onboarding: AddBubblePlay");
            enableOnboarding = false;

            if (soundPlayerNothingHappening) soundPlayerNothingHappening.Stop();

            foreach (GameObject go in bubblesInOrder)
            {
                go.GetComponent<EasedScaling>().visible = false;
            }

            StartCoroutine(PlayAudioAndWait(FirstBubbleTouched, () =>
            {

                //Onboarding is done
                Log.d("onboarding done");
                EnableBubbles();

            }));

        }
        else
        {
            Log.d("AddBubblePlay" + bubblesPlayed.Count);

            //add the currently played bubble to the list
            bubblesPlayed.Add(bubble);


            //disable bubble trigger
            bubble.GetComponent<SphereCollider>().enabled = false;

            //make bubble invisible
            //bubble.GetComponent<Animator>().Play("ScaleDown");
            bubble.GetComponent<EasedScaling>().visible = false;

            //indicate another bubble played in the UI
            if (allBubblesPlayedForOnce) UIPlayCountContainer.transform.GetChild(bubblesPlayed.Count - 1).GetChild(0).gameObject.SetActive(true);


            // if we have played all bubbles that are there to play
            if (bubblesPlayed.Count == bubblesInOrder.Length)
            {
                // if we're doing this for the first time, play audio and the correct melody

                if (!allBubblesPlayedForOnce)
                {
                    Log.d("AllBubblesPlayedOnce");
                    StartCoroutine(PlayAudioAndWait(AllBubblesPlayedOnce, () =>
                    {
                        EnableBubbles();
                        allBubblesPlayedForOnce = true;
                        //ready to reveal UI

                        if (GlobalSettings.instance.IsEnglish)
                        {
                            replayAudioButtonEN.SetActive(true);
                        }
                        else
                        {
                            replayAudioButtonDE.SetActive(true);
                        }

                        //reset play count indicator in UI
                        ResetUIPlayCountIndicator();

                    }));

                }
                else
                {
                   

                    // if not, let's check if it's been played in the right order
                    CheckIfSameOrder();

                } 
               
            }
            else
            {
                // if we're not there yet
                Log.d("One Bubble less playable");
            }  
        }
    }


    public void CheckIfSameOrder()
    {
        Log.d("CheckIfSameOrder");

        int i = 0;
        int same = 0;
        int notSame = 0;

        foreach (GameObject go in bubblesPlayed)
        {
            //Log.d(go.name + " is compared to " + bubblesInOrder[i].name);

            if (go.Equals(bubblesInOrder[i]))
            {
                same++;               
            }
            else
            {
                notSame++;
            }
            
            i++;
        }


        if (same == bubblesInOrder.Length)
        {
            Success();
        }
        else
        {
            Log.d("Bubbles were NOT played in correct order.");
            EnableBubbles();

            //reset play count indication in UI
            ResetPlayCount();

            // removed for less annoyance
            //StartCoroutine(PlayAudioAndWait(NotRightOrder, () =>
            //{
            //    EnableBubbles();
            //}, 0, 1.5f));
        }
    }
    void ResetPlayCount()
    {
        Log.d("ResetPlayCount");
        foreach (Transform child in UIPlayCountContainer.transform)
        {
            child.GetChild(0).gameObject.SetActive(false);
            child.GetComponent<Animator>().Play("FadeOut");
        }
    }

    void ResetUIPlayCountIndicator()
    {


        UIPlayCountContainer.SetActive(true);

        // remove any children, if already instantiated some time ago
        //if (UIPlayCountContainer.transform.childCount > 0) foreach (Transform child in UIPlayCountContainer.transform) Destroy(child.gameObject);

        // adjust number of indicators to bubbles in order.length
        for (var i = 0; i < bubblesInOrder.Length; i++)
        {
            var UIPlayCountIndicator = Instantiate(UIPlayCountPrefab, UIPlayCountContainer.transform);

            // disable playcount indicator child UI image
            UIPlayCountIndicator.transform.GetChild(0).gameObject.SetActive(false);

            // play animation
            UIPlayCountIndicator.GetComponent<Animator>().Play("FadeIn");
        }

    }


    void Success()
    {
        Log.d("Success! Bubbles were played in correct order.");

       
        replayAudioButtonEN.SetActive(false);
        replayAudioButtonDE.SetActive(false);

        UIPlayCountContainer.SetActive(false);

        foreach (GameObject bubble in bubblesInOrder)
        {
            //go.GetComponent<Animator>().Play("ScaleDown");
            bubble.GetComponent<EasedScaling>().visible = false;

            // enable Particle System
            //go.transform.GetChild(0).gameObject.SetActive(true);
            bubble.GetComponentInChildren<ParticleSystem>(true).gameObject.SetActive(true);

            bubble.GetComponent<SphereCollider>().enabled = false;
            bubble.GetComponent<TriggerSoundOnApproach>().enabled = false;
        }


        StartCoroutine(PlayAudioAndWait(CelebrationRightOrder, () =>
        {
            StoryChat.instance._AR_success();
        }));
    }

    void EnableBubbles(bool enableCollider = true)
    {
       
        bubblesPlayed = new ArrayList();
        foreach (GameObject bubble in bubblesInOrder)
        {

            EnableBubble(bubble, enableCollider);
        }
    }

    void EnableFirstBubble(bool enableCollider = true)
    {


        bubblesPlayed = new ArrayList();

        EnableBubble(bubblesInOrder[0], enableCollider);

    }


    void EnableBubble (GameObject bubble, bool enableCollider = true)
    {

        //bubble.GetComponent<Animator>().Play("ScaleUp");
        bubble.GetComponent<EasedScaling>().visible = true;

        // disable Particle System
        //bubble.transform.GetChild(0).gameObject.SetActive(false);
        bubble.GetComponentInChildren<ParticleSystem>(true).gameObject.SetActive(false);

        // don't enable colliders immediately, only after scale up is done
        // scaling up may interfere with phone
        // roughly a second??
        // TODO: do this when easing or animator is done
        // TODO: use DigitalRuby.Tween
        StartCoroutine(DelayAction(1, () => {
            bubble.GetComponent<SphereCollider>().enabled = enableCollider;
            bubble.GetComponent<TriggerSoundOnApproach>().enabled = enableCollider;
        }));
    }


    private IEnumerator PlayAudioAndWait(AudioSourceLanguage audio, Action action = null, float delay = 0, float shortenWait = 0)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        audio.Play();

        if (audio.clip)
        {
            yield return new WaitForSeconds(audio.clip.length - shortenWait);
        }

        if (action != null)
        {
            action();
        }
    }

    private IEnumerator DelayAction(float time, Action action = null)
    {
        yield return new WaitForSeconds(time);

        if (action != null)
        {
            action();
        }
    }

}
