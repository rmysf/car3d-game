using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class mobil : MonoBehaviour
{
    [Header("--- JOYSTICK ---")]
    public FixedJoystick joystick;

    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentBrakeForce;
    private bool isBraking;

    [Header("--- SETTINGS ---")]
    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float maxSteerAngle = 30f;

    [Header("--- WHEEL COLLIDERS ---")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [Header("--- WHEEL MESHES ---")]
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    [Header("--- UI ---")]
    [SerializeField] private TextMeshProUGUI teksTimer;
    [SerializeField] private TextMeshProUGUI teksCountdown;
    [SerializeField] private GameObject panelGameOver;       // panel utama game over
    [SerializeField] private TextMeshProUGUI textGameOver;   // teks di dalam PanelGameOver (judul + waktu)

    [HideInInspector] public bool gameOver  = false;
    [HideInInspector] public bool gameReady = false;

    [Header("--- COUNTDOWN ---")]
    [SerializeField] private float durasiCountdown = 3f;

    private float timerSurvive = 0f;
    private float countdown;

    void Start()
    {
        countdown = durasiCountdown;

        // Pastikan PanelGameOver hilang saat game baru mulai
        if (panelGameOver != null)
            panelGameOver.SetActive(false);

        if (teksTimer != null)
            teksTimer.text = "Survive: 00:00";
    }

    void Update()
    {
        if (gameOver)
        {
            // Mobile tidak punya tombol R, tapi tetap support keyboard untuk testing di editor
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        // FASE COUNTDOWN
        if (!gameReady)
        {
            countdown -= Time.deltaTime;

            if (teksCountdown != null)
            {
                int angka = Mathf.CeilToInt(countdown);
                if (angka > 0)
                {
                    teksCountdown.text  = angka.ToString();
                    teksCountdown.color = Color.white;
                }
                else
                {
                    teksCountdown.text  = "KABUR!";
                    teksCountdown.color = new Color(1f, 0.63f, 0.15f); // kuning/oranye
                }
            }

            if (countdown <= 0f)
            {
                gameReady = true;
                Invoke(nameof(SembunyikanCountdown), 0.5f);
            }

            return;
        }

        // FASE GAME - hitung timer survive
        timerSurvive += Time.deltaTime;

        if (teksTimer != null)
        {
            int menit = Mathf.FloorToInt(timerSurvive / 60f);
            int detik = Mathf.FloorToInt(timerSurvive % 60f);
            teksTimer.text = string.Format("Survive: {0:00}:{1:00}", menit, detik);
        }
    }

    void SembunyikanCountdown()
    {
        if (teksCountdown != null)
            teksCountdown.gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!gameReady || gameOver)
        {
            // Kunci mobil selama countdown / game over
            rearLeftWheelCollider.motorTorque  = 0f;
            rearRightWheelCollider.motorTorque = 0f;
            frontLeftWheelCollider.brakeTorque  = brakeForce;
            frontRightWheelCollider.brakeTorque = brakeForce;
            rearLeftWheelCollider.brakeTorque   = brakeForce;
            rearRightWheelCollider.brakeTorque  = brakeForce;
            return;
        }

        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        // Prioritas joystick; fallback ke keyboard (berguna saat testing di editor)
        if (joystick != null)
        {
            horizontalInput = joystick.Horizontal;
            verticalInput   = joystick.Vertical;
        }
        else
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput   = Input.GetAxis("Vertical");
        }

        // Joystick tidak punya tombol rem — Space hanya aktif di editor/keyboard
        isBraking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        if (isBraking)
        {
            rearLeftWheelCollider.motorTorque  = 0f;
            rearRightWheelCollider.motorTorque = 0f;
            currentBrakeForce = brakeForce;
        }
        else
        {
            rearLeftWheelCollider.motorTorque  = verticalInput * motorForce;
            rearRightWheelCollider.motorTorque = verticalInput * motorForce;
            currentBrakeForce = 0f;
        }
        ApplyBraking();
    }

    private void ApplyBraking()
    {
        frontLeftWheelCollider.brakeTorque  = currentBrakeForce;
        frontRightWheelCollider.brakeTorque = currentBrakeForce;
        rearLeftWheelCollider.brakeTorque   = currentBrakeForce;
        rearRightWheelCollider.brakeTorque  = currentBrakeForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle  = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider,  frontLeftWheelTransform,  true);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform, false);
        UpdateSingleWheel(rearLeftWheelCollider,   rearLeftWheelTransform,   true);
        UpdateSingleWheel(rearRightWheelCollider,  rearRightWheelTransform,  false);
    }

    private void UpdateSingleWheel(WheelCollider wc, Transform wt, bool isLeft)
    {
        Vector3 pos; Quaternion rot;
        wc.GetWorldPose(out pos, out rot);
        wt.position = pos;
        wt.rotation = isLeft ? rot * Quaternion.Euler(0, 180, 0) : rot;
    }

    // Dipanggil dari script lain (misal: trigger tabrakan / ketangkap)
    public void TriggerGameOver()
    {
        if (gameOver) return;
        gameOver = true;

        rearLeftWheelCollider.motorTorque  = 0f;
        rearRightWheelCollider.motorTorque = 0f;
        frontLeftWheelCollider.brakeTorque  = brakeForce;
        frontRightWheelCollider.brakeTorque = brakeForce;
        rearLeftWheelCollider.brakeTorque   = brakeForce;
        rearRightWheelCollider.brakeTorque  = brakeForce;

        // Pastikan teks countdown awal sudah tidak tampil lagi
        if (teksCountdown != null)
            teksCountdown.gameObject.SetActive(false);

        // Tampilkan PanelGameOver saat player tertangkap
        if (panelGameOver != null)
            panelGameOver.SetActive(true);

        if (textGameOver != null)
        {
            int menit = Mathf.FloorToInt(timerSurvive / 60f);
            int detik = Mathf.FloorToInt(timerSurvive % 60f);
            textGameOver.text = string.Format(
                "<size=52>TERTANGKAP!</size>\n\n<color=#EF9F27><size=42>Survive: {0:00}:{1:00}</size></color>",
                menit, detik
            );
        }
    }

    // Panggil dari tombol UI restart (untuk mobile)
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}