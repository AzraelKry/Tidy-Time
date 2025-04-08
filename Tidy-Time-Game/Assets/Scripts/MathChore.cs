using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class MathChore : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text[] questionTexts = new TMP_Text[5];
    public TMP_InputField[] answerInputs = new TMP_InputField[5];
    public Button[] checkButtons = new Button[5];
    public GameObject completionPanel;

    [Header("Colors")]
    public Color correctColor = new Color(120f / 255f, 255f / 255f, 120f / 255f);
    public Color incorrectColor = new Color(255f / 255f, 120f / 255f, 120f / 255f);
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

    private bool firstSetCompleted = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        GenerateQuestions();

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
        HashSet<string> usedQuestions = new HashSet<string>();

        while (questions.Count < 5)
        {
            string op = operators[Random.Range(0, operators.Length)];
            int num1, num2, result;
            bool validQuestion = false;

            while (!validQuestion)
            {
                // For subtraction, we need to ensure num1 >= num2
                if (op == "-")
                {
                    // Generate num1 first (2-9, with 5% chance for 0-1)
                    num1 = Random.value < 0.05f ? Random.Range(0, 2) : Random.Range(2, 10);

                    // Then generate num2 that's <= num1
                    // 5% chance for 0 or 1, 95% chance for 2 up to num1
                    num2 = Random.value < 0.05f ? Random.Range(0, Mathf.Min(2, num1 + 1)) : Random.Range(2, num1 + 1);
                    result = num1 - num2;
                    validQuestion = true; // Guaranteed non-negative since num2 <= num1
                }
                else // Addition
                {
                    // 5% chance for 0 or 1, 95% chance for 2-9
                    num1 = Random.value < 0.05f ? Random.Range(0, 2) : Random.Range(2, 10);

                    // For addition, ensure sum doesn't exceed 9
                    int maxSecondNum = 9 - num1;
                    if (maxSecondNum <= 0)
                    {
                        continue; // Skip this iteration and try again
                    }

                    // 5% chance for 0 or 1, 95% chance for 2-maxSecondNum
                    num2 = Random.value < 0.05f ? Random.Range(0, Mathf.Min(2, maxSecondNum + 1)) : Random.Range(2, maxSecondNum + 1);
                    result = num1 + num2;

                    // Double-check that result is <= 9
                    if (result > 9) continue;
                    validQuestion = true;
                }

                if (validQuestion)
                {
                    string question = $"{num1} {op} {num2} = ";
                    if (!usedQuestions.Contains(question))
                    {
                        usedQuestions.Add(question);
                        questions.Add(question);
                        correctAnswers.Add(result);
                        validQuestion = true;
                    }
                    else
                    {
                        validQuestion = false;
                    }
                }
            }
        }

        for (int i = 0; i < 5; i++)
        {
            questionTexts[i].text = questions[i];
        }
    }

    private void GenerateAdvancedQuestions()
    {
        questions.Clear();
        correctAnswers.Clear();
        HashSet<string> usedQuestions = new HashSet<string>();
        string[] operators = { "+", "-", "*" };

        // First ensure we have at least 1 multiplication question with result ≤ 9
        bool hasMultiplication = false;
        while (!hasMultiplication)
        {
            int num1 = Random.value < 0.05f ? Random.Range(0, 2) : Random.Range(2, 4); // Limit first number for multiplication
            int num2 = Random.value < 0.05f ? Random.Range(0, 2) : Random.Range(2, 4); // Limit second number for multiplication
            int result = num1 * num2;

            if (result <= 9)
            {
                string question = $"{num1} * {num2} = ";
                if (!usedQuestions.Contains(question))
                {
                    usedQuestions.Add(question);
                    questions.Add(question);
                    correctAnswers.Add(result);
                    hasMultiplication = true;
                }
            }
        }

        // Generate remaining 4 questions (can be any type) with results ≤ 9 and no negatives
        while (questions.Count < 5)
        {
            string op = operators[Random.Range(0, operators.Length)];
            int num1, num2, result;
            bool validQuestion = false;

            while (!validQuestion)
            {
                if (op == "-")
                {
                    // For subtraction, generate num1 first
                    num1 = Random.value < 0.05f ? Random.Range(0, 2) : Random.Range(2, 10);

                    // Then generate num2 that's <= num1
                    num2 = Random.value < 0.05f ? Random.Range(0, Mathf.Min(2, num1 + 1)) : Random.Range(2, num1 + 1);
                    result = num1 - num2;
                    validQuestion = true; // Guaranteed non-negative
                }
                else if (op == "+")
                {
                    // 5% chance for 0 or 1, 95% chance for 2-9
                    num1 = Random.value < 0.05f ? Random.Range(0, 2) : Random.Range(2, 10);

                    // Ensure sum doesn't exceed 9
                    int maxSecondNum = 9 - num1;
                    if (maxSecondNum <= 0)
                    {
                        continue;
                    }

                    num2 = Random.value < 0.05f ? Random.Range(0, Mathf.Min(2, maxSecondNum + 1)) : Random.Range(2, maxSecondNum + 1);
                    result = num1 + num2;
                    validQuestion = result <= 9;
                }
                else // Multiplication
                {
                    // 5% chance for 0 or 1, 95% chance for 2-9
                    num1 = Random.value < 0.05f ? Random.Range(0, 2) : Random.Range(2, 10);

                    // Limit second number to keep product ≤ 9
                    int maxSecondNum = num1 == 0 ? 9 : Mathf.FloorToInt(9f / num1);
                    if (maxSecondNum < 2)
                    {
                        continue;
                    }

                    num2 = Random.value < 0.05f ? Random.Range(0, 2) : Random.Range(2, maxSecondNum + 1);
                    result = num1 * num2;
                    validQuestion = result <= 9;
                }

                if (validQuestion)
                {
                    string question = $"{num1} {op} {num2} = ";
                    if (!usedQuestions.Contains(question))
                    {
                        usedQuestions.Add(question);
                        questions.Add(question);
                        correctAnswers.Add(result);
                        validQuestion = true;
                    }
                    else
                    {
                        validQuestion = false;
                    }
                }
            }
        }

        foreach (TMP_Text text in questionTexts)
        {
            text.ForceMeshUpdate();
        }

        for (int i = 0; i < 5; i++)
        {
            questionTexts[i].text = questions[i];
        }

        completedQuestions = new bool[5];
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
            // Clear existing listeners
            answerInputs[i].onValueChanged.RemoveAllListeners();
            answerInputs[i].onSelect.RemoveAllListeners();
            answerInputs[i].onDeselect.RemoveAllListeners();
            checkButtons[i].onClick.RemoveAllListeners();

            // Add new listeners
            int index = i;
            answerInputs[i].onValueChanged.AddListener(delegate { OnInputChanged(index); });
            answerInputs[i].onSelect.AddListener(delegate { OnInputSelected(index); });
            checkButtons[i].onClick.AddListener(delegate { OnCheckButtonClicked(index); });
        }
    }

    private void OnInputSelected(int index)
    {
        // Clear the input field when selected
        answerInputs[index].text = "";
    }

    private void OnInputChanged(int index)
    {
        ValidateInput(answerInputs[index]);
    }

    private void OnCheckButtonClicked(int index)
    {
        // Force input processing before checking answer
        if (answerInputs[index].isFocused)
        {
            answerInputs[index].ReleaseSelection();
            answerInputs[index].OnDeselect(null);
        }
        CheckAnswer(index);
    }

    private void ValidateInput(TMP_InputField inputField)
    {
        if (string.IsNullOrEmpty(inputField.text)) return;

        // Filter out non-digit characters (including negative signs)
        string cleanedInput = new string(inputField.text.Where(c => char.IsDigit(c)).ToArray());

        // Only keep the last character entered (replace mode)
        if (cleanedInput.Length > 0)
        {
            inputField.text = cleanedInput[cleanedInput.Length - 1].ToString();
        }
        else
        {
            inputField.text = "";
        }
    }

    private void UpdateButtonState(int questionIndex, bool isCorrect)
    {
        if (completedQuestions[questionIndex]) return;
        UpdateButtonColor(checkButtons[questionIndex], isCorrect ? correctColor : incorrectColor);
    }

    private void CheckAnswer(int questionIndex)
    {
        if (completedQuestions[questionIndex]) return;

        bool isCorrect = !string.IsNullOrEmpty(answerInputs[questionIndex].text) &&
                        int.TryParse(answerInputs[questionIndex].text, out int answer) &&
                        answer == correctAnswers[questionIndex];

        // Update button color based on correctness
        UpdateButtonState(questionIndex, isCorrect);

        if (isCorrect)
        {
            if (correctSound != null && correctSound.isActiveAndEnabled)
                correctSound.Play();

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
            if (incorrectSound != null && incorrectSound.isActiveAndEnabled)
                incorrectSound.Play();

            StartCoroutine(ShakeButton(checkButtons[questionIndex]));
        }
    }

    private IEnumerator ShakeButton(Button button)
    {
        if (button == null) yield break;

        RectTransform rt = button.GetComponent<RectTransform>();
        if (rt == null) yield break;

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
        if (button == null) return;

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

        if (!firstSetCompleted)
        {
            firstSetCompleted = true;
            SwitchToAdvancedSet();
        }
        else
        {
            CompleteMathChore();
        }
    }

    private void SwitchToAdvancedSet()
    {
        GenerateAdvancedQuestions();
        SetupUI();
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
        if (questions.Count == 0)
        {
            GenerateQuestions();
        }

        for (int i = 0; i < 5; i++)
        {
            questionTexts[i].text = questions[i];
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
            if (!completedQuestions[i] && !string.IsNullOrEmpty(answerInputs[i].text))
                CheckAnswer(i);
        }
    }
}