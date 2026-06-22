using UnityEngine;

public class PoliceCarAI : MonoBehaviour
{
    [Header("--- TARGET ---")]
    [SerializeField] private Transform targetPlayer;

    [Header("--- WHEEL COLLIDERS ---")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [Header("--- WHEEL TRANSFORMS ---")]
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    [Header("--- KECEPATAN DINAMIS ---")]
    [SerializeField] private float motorForceAwal     = 500f;   // tenaga awal lemah
    [SerializeField] private float motorForceMaksimal = 2000f;  // tenaga maksimal
    [SerializeField] private float brakeForce         = 2000f;
    [SerializeField] private float maxSteerAngle      = 35f;
    [SerializeField] private float selisihDariPlayer  = 3f;     // polisi selalu X m/s lebih lambat
    [SerializeField] private float kecepatanAdaptasi  = 1.5f;   // seberapa cepat naik tenaga

    [Header("--- TANGKAP ---")]
    [SerializeField] private float jarakTangkap      = 3f;
    [Tooltip("Kekuatan tabrakan minimum untuk game over. Serempet pelan = polisi tetap ngejar.")]
    [SerializeField] private float minImpulseTangkap = 500f;

    [Header("--- EFEK SIRINE (Lampu) ---")]
    [SerializeField] private Light lampuSiren;
    [SerializeField] private float kecepatanKedip = 3f;

    [Header("--- EFEK SIRINE (Audio) ---")]
    [SerializeField] private AudioSource audioSirine;
    [SerializeField] private AudioClip   klipSirine;
    [SerializeField] private AudioClip   klipTangkap;

    private mobil     playerScript;
    private Rigidbody playerRb;
    private Rigidbody policeRb;
    private bool      sudahTangkap   = false;
    private float     timerKedip     = 0f;
    private bool      warnaKedipBiru = true;
    private float     motorForceSaatIni;

    void Start()
    {
        policeRb = GetComponent<Rigidbody>();
        if (policeRb != null)
            policeRb.centerOfMass = new Vector3(0, -0.5f, 0);

        if (targetPlayer == null)
        {
            GameObject obj = GameObject.FindWithTag("Player");
            if (obj != null)
            {
                targetPlayer = obj.transform;
                playerScript = obj.GetComponent<mobil>();
                playerRb     = obj.GetComponent<Rigidbody>();
            }
            else
                Debug.LogWarning("[PoliceCarAI] Tag 'Player' tidak ditemukan!");
        }
        else
        {
            playerScript = targetPlayer.GetComponent<mobil>();
            playerRb     = targetPlayer.GetComponent<Rigidbody>();
        }

        motorForceSaatIni = motorForceAwal;

        if (lampuSiren != null)
            lampuSiren.enabled = false;

        if (audioSirine != null && klipSirine != null)
        {
            audioSirine.clip = klipSirine;
            audioSirine.loop = true;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();
    }

    void Update()
    {
        if (sudahTangkap) return;
        if (targetPlayer == null || playerScript == null) return;
        if (!playerScript.gameReady) return;
        if (playerScript.gameOver) return;

        EfekSiren(true);
        MulaiSuaraSirine();

        float jarak = Vector3.Distance(transform.position, targetPlayer.position);
        if (jarak <= jarakTangkap)
            TangkapPlayer();
    }

    void FixedUpdate()
    {
        if (sudahTangkap) return;
        if (targetPlayer == null || playerScript == null) return;
        if (!playerScript.gameReady) return;
        if (playerScript.gameOver) return;

        SesuaikanTenaga();
        HandleSteering();
        HandleMotor();
        UpdateWheels();
    }

    // -------------------------------------------------------
    // Tenaga mesin naik mengikuti kecepatan player
    // -------------------------------------------------------
    void SesuaikanTenaga()
    {
        float kecepatanPlayer = playerRb != null ? playerRb.linearVelocity.magnitude : 0f;
        float kecepatanPolisi = policeRb != null ? policeRb.linearVelocity.magnitude : 0f;

        float targetKecepatan = Mathf.Max(0f, kecepatanPlayer - selisihDariPlayer);

        float targetForce = kecepatanPolisi < targetKecepatan
            ? motorForceMaksimal
            : motorForceAwal;

        motorForceSaatIni = Mathf.Lerp(
            motorForceSaatIni,
            targetForce,
            Time.deltaTime * kecepatanAdaptasi
        );

        motorForceSaatIni = Mathf.Clamp(motorForceSaatIni, motorForceAwal, motorForceMaksimal);
    }

    // -------------------------------------------------------
    // Kemudi otomatis ke arah player
    // -------------------------------------------------------
    void HandleSteering()
    {
        Vector3 arahLokal = transform.InverseTransformPoint(targetPlayer.position);

        float sudut = Mathf.Atan2(arahLokal.x, arahLokal.z) * Mathf.Rad2Deg;
        float targetSteer = Mathf.Clamp(sudut, -maxSteerAngle, maxSteerAngle);

        frontLeftWheelCollider.steerAngle  = targetSteer;
        frontRightWheelCollider.steerAngle = targetSteer;
    }

    // -------------------------------------------------------
    // Gas & rem otomatis
    // -------------------------------------------------------
    void HandleMotor()
    {
        Vector3 arahLokal = transform.InverseTransformPoint(targetPlayer.position);
        bool playerDiDepan = arahLokal.z > 0;

        if (playerDiDepan)
        {
            rearLeftWheelCollider.motorTorque  = motorForceSaatIni;
            rearRightWheelCollider.motorTorque = motorForceSaatIni;
            rearLeftWheelCollider.brakeTorque  = 0f;
            rearRightWheelCollider.brakeTorque = 0f;
            frontLeftWheelCollider.brakeTorque  = 0f;
            frontRightWheelCollider.brakeTorque = 0f;
        }
        else
        {
            rearLeftWheelCollider.motorTorque  = 0f;
            rearRightWheelCollider.motorTorque = 0f;
            rearLeftWheelCollider.brakeTorque  = brakeForce;
            rearRightWheelCollider.brakeTorque = brakeForce;
            frontLeftWheelCollider.brakeTorque  = brakeForce;
            frontRightWheelCollider.brakeTorque = brakeForce;
        }
    }

    // -------------------------------------------------------
    // Sinkronisasi mesh roda dengan WheelCollider
    // -------------------------------------------------------
    void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider,  frontLeftWheelTransform,  true);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform, false);
        UpdateSingleWheel(rearLeftWheelCollider,   rearLeftWheelTransform,   true);
        UpdateSingleWheel(rearRightWheelCollider,  rearRightWheelTransform,  false);
    }

    void UpdateSingleWheel(WheelCollider wc, Transform wt, bool isLeft)
    {
        if (wt == null) return;
        Vector3 pos; Quaternion rot;
        wc.GetWorldPose(out pos, out rot);
        wt.position = pos;
        wt.rotation = isLeft ? rot * Quaternion.Euler(0, 180, 0) : rot;
    }

    // -------------------------------------------------------
    // Efek sirine (lampu)
    // -------------------------------------------------------
    void EfekSiren(bool aktif)
    {
        if (lampuSiren == null) return;
        lampuSiren.enabled = aktif;
        if (!aktif) return;

        timerKedip += Time.deltaTime * kecepatanKedip;
        if (timerKedip >= 1f)
        {
            timerKedip = 0f;
            warnaKedipBiru = !warnaKedipBiru;
            lampuSiren.color = warnaKedipBiru ? Color.blue : Color.red;
        }
    }

    // -------------------------------------------------------
    // Efek sirine (audio)
    // -------------------------------------------------------
    void MulaiSuaraSirine()
    {
        if (audioSirine == null || klipSirine == null) return;
        if (!audioSirine.isPlaying)
            audioSirine.Play();
    }

    void HentikanSuaraSirine()
    {
        if (audioSirine == null) return;
        audioSirine.Stop();
    }

    // -------------------------------------------------------
    // Tangkap player
    // -------------------------------------------------------
    void TangkapPlayer()
    {
        if (sudahTangkap) return;
        sudahTangkap = true;

        rearLeftWheelCollider.motorTorque  = 0f;
        rearRightWheelCollider.motorTorque = 0f;
        frontLeftWheelCollider.brakeTorque  = brakeForce;
        frontRightWheelCollider.brakeTorque = brakeForce;
        rearLeftWheelCollider.brakeTorque   = brakeForce;
        rearRightWheelCollider.brakeTorque  = brakeForce;

        HentikanSuaraSirine();
        if (audioSirine != null && klipTangkap != null)
            audioSirine.PlayOneShot(klipTangkap);

        Debug.Log("[PoliceCarAI] Player tertangkap!");
        if (playerScript != null)
            playerScript.TriggerGameOver();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        float kekuatanTabrakan = collision.impulse.magnitude;
        if (kekuatanTabrakan > minImpulseTangkap)
            TangkapPlayer();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, jarakTangkap);
    }
}