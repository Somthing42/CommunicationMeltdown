using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceRec : MonoBehaviour {

    KeywordRecognizer keywordRecognizer;
    // keyword array
    private string[] Keywords_array = { "current status" };
    public GameObject explodeParts;
    public Transform explodePos;

   

    // Use this for initialization
    void Start()
    {
       
        // instantiate keyword recognizer, pass keyword array in the constructor
        keywordRecognizer = new KeywordRecognizer(Keywords_array);
        keywordRecognizer.OnPhraseRecognized += OnKeywordsRecognized;
        // start keyword recognizer
        keywordRecognizer.Start();
    }

    void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {

        if(args.text == "current status")
        {
            Instantiate(explodeParts, explodePos.transform.position, Quaternion.identity);
        }

    }
}