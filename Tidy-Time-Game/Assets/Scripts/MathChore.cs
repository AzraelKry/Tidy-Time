using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MathChore : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text[] questionTexts = new TMP_Text[5];
    public TMP_InputField[] answerInputs = new TMP_InputField[5];
    public Button[] checkButtons = new Button[5];
    public GameObject completionPanel;

    [Header("Colors")]
    public Color correctColor = new Color(120f/255f, 255f/255f, 120f/255f);
    public Color incorrectColor = new Color(255f/255f, 120f/255f, 120f/255f);
    public Color defaultColor = Color.white;
    public Color disabledColor = new Color(0.8f, 0.8f, 0.8f);
    [Range(0, 1)] public float highlightIntensity = 0.7f;

    [Header("Animation Settings")]
    public float wrongAnswerShakeDuration = 0.25f;
    public float wrongAnswerShakeIntensity = 5f;

    [Header("Audio")]
    public AudioSource correctSound;
    public AudioSource incorrectSound;
    private AudioSource audioSource;

    private bool[] completedQuestions = new bool[5];
    private List<string> questions = new List<string>();
    private List<int> correctAnswers = new List<int>();

    private void Start()
    {
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Always generate questions first
        GenerateQuestions();

        // Then check completion state
        if (ChoreManager.Instance != null && ChoreManager.Instance.isMathHomeworkCompleted)
        {
            ShowCompletionState();
            return;
        }

        InitializeMathGame();
    }

    private void InitializeMathGame()
    {
        SetupUI();
        AddEventListeners();
    }

    private void GenerateQuestions()
    {
        questions.Clear();
        correctAnswers.Clear();
        string[] operators = { "+", "-" };

        for (int i = 0; i < 5; i++)
        {
            string op = operators[Random.Range(0, operators.Length)];
            int num1 = Random.Range(1, 9);
            int num2 = op == "+" ? Random.Range(1, 9 - num1) : Random.Range(1, num1);

            questions.Add($"{num1} {op} {num2} = ");
            correctAnswers.Add(op == "+" ? num1 + num2 : num1 - num2);
            questionTexts[i].text = questions[i];
        }
    }

    private void SetupUI()
    {
        for (int i = 0; i < 5; i++)
        {
            answerInputs[i].text = "";
            answerInputs[i].interactable = true;
            checkButtons[i].interactable = true;
            UpdateButtonColor(checkButtons[i], defaultColor);
        }

        if (completionPanel != null)
            completionPanel.SetActive(false);
    }

    private void AddEventListeners()
    {
        for (int i = 0; i < 5; i++)
        {
            int index = i;
            answerInputs[i].onValueChanged.AddListener(_ => ValidateInput(answerInputs[index]));
            checkButtons[i].onClick.AddListener(() => CheckAnswer(index));
        }
    }

    private void ValidateInput(TMP_InputField inputField)
    {
        if (inputField.text.Length > 0)
        {
            if (!char.IsDigit(inputField.text[0]))
            {
                inputField.text = "";
                return;
            }
            
            int num = int.Parse(inputField.text);
            if (num < 0 || num > 9)
                inputField.text = "";
        }
    }

    private void CheckAnswer(int questionIndex)
    {
        if (completedQuestions[questionIndex]) return;

        bool isCorrect = int.TryParse(answerInputs[questionIndex].text, out int answer) && 
                        answer == correctAnswers[questionIndex];

        if (isCorrect)
        {
            if (correctSound != null) correctSound.Play();

            completedQuestions[questionIndex] = true;
            answerInputs[questionIndex].interactable = false;
            checkButtons[questionIndex].interactable = false;
            
            var colors = checkButtons[questionIndex].colors;
            colors.disabledColor = correctColor;
            checkButtons[questionIndex].colors = colors;
            checkButtons[questionIndex].GetComponent<Image>().color = correctColor;

            CheckAllAnswersCompleted();
        }
        else
        {
            if (incorrectSound != null) incorrectSound.Play();

            UpdateButtonColor(checkButtons[questionIndex], incorrectColor);
            checkButtons[questionIndex].GetComponent<Image>().color = incorrectColor;
            StartCoroutine(ShakeButton(checkButtons[questionIndex]));
        }
    }

    private IEnumerator ShakeButton(Button button)
    {
        RectTransform rt = button.GetComponent<RectTransform>();
        Vector2 originalPos = rt.anchoredPosition;
        float elapsed = 0f;
        
        while (elapsed < wrongAnswerShakeDuration)
        {
            float x = Random.Range(-1f, 1f) * wrongAnswerShakeIntensity;
            float y = Random.Range(-1f, 1f) * wrongAnswerShakeIntensity;
            rt.anchoredPosition = originalPos + new Vector2(x, y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        rt.anchoredPosition = originalPos;
    }

    private void UpdateButtonColor(Button button, Color color)
    {
        var colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, highlightIntensity);
        colors.pressedColor = Color.Lerp(color, Color.black, highlightIntensity);
        colors.selectedColor = color;
        button.colors = colors;
        button.GetComponent<Image>().color = color;
    }

    private void CheckAllAnswersCompleted()
    {
        foreach (bool completed in completedQuestions)
        {
            if (!completed) return;
        }

        CompleteMathChore();
    }

    private void CompleteMathChore()
    {
        if (completionPanel != null)
            completionPanel.SetActive(true);

        if (ChoreManager.Instance != null)
            ChoreManager.Instance.CompleteChore("MathHomework");
    }

    private void ShowCompletionState()
    {
        // First ensure we have questions generated
        if (questions.Count == 0)
        {
            GenerateQuestions();
        }

        // Show completed state
        for (int i = 0; i < 5; i++)
        {
            questionTexts[i].text = questions[i]; // Keep original question text
            answerInputs[i].text = correctAnswers[i].ToString();
            answerInputs[i].interactable = false;
            checkButtons[i].interactable = false;
            
            var colors = checkButtons[i].colors;
            colors.disabledColor = correctColor;
            checkButtons[i].colors = colors;
            checkButtons[i].GetComponent<Image>().color = correctColor;
        }

        if (completionPanel != null)
            completionPanel.SetActive(true);
    }

    public void CheckAllAnswers()
    {
        for (int i = 0; i < 5; i++)
        {
            if (!completedQuestions[i])
                CheckAnswer(i);
        }
    }
}