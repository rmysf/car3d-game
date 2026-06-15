using UnityEngine;

/// <summary>
/// Menyimpan dan mendistribusikan input dari berbagai sumber
/// (joystick UI, touch camera, tombol UI).
/// Tidak bergantung pada UnityEngine.InputSystem — aman untuk proyek
/// yang hanya pakai Input System lama (Input.GetAxis / Touch).
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    // ── Movement (diisi oleh FixedJoystick / joystick UI) ──────────────
    public Vector2 MovementInput { get; private set; }

    public void SetMovementInput(Vector2 input)
    {
        MovementInput = input;
    }

    // ── Rotation / Camera (diisi oleh MobileCameraTouch) ───────────────
    public Vector2 RotationInput { get; private set; }

    public void SetRotationInput(Vector2 input)
    {
        RotationInput = input;
    }

    // ── Tombol-tombol (diisi dari tombol UI OnClick / OnPointerDown) ────
    public bool JumpTriggered       { get; private set; }
    public bool SprintTriggered     { get; private set; }
    public bool FireTriggered       { get; private set; }
    public bool ToggleWeaponTriggered { get; private set; }
    public bool RotateObjectTriggered { get; private set; }
    public bool ReloadTriggered     { get; private set; }

    // Setter publik — panggil dari tombol UI (EventTrigger / UnityEvent)
    public void SetJump(bool value)         => JumpTriggered = value;
    public void SetSprint(bool value)       => SprintTriggered = value;
    public void SetFire(bool value)         => FireTriggered = value;
    public void SetToggleWeapon(bool value) => ToggleWeaponTriggered = value;
    public void SetRotateObject(bool value) => RotateObjectTriggered = value;
    public void SetReload(bool value)       => ReloadTriggered = value;

    /// <summary>
    /// Consume flag ToggleWeapon — mencegah kelas lain perlu akses setter.
    /// Kembalikan true sekali lalu reset ke false.
    /// </summary>
    public bool ConsumeToggleWeaponTriggered()
    {
        if (!ToggleWeaponTriggered) return false;
        ToggleWeaponTriggered = false;
        return true;
    }

    // ── (Opsional) Reset semua flag tiap akhir frame ───────────────────
    // Aktifkan blok ini kalau tombol UI kamu hanya kirim event sesaat
    // (bukan hold), supaya flag tidak nyangkut true selamanya.
    //
    // void LateUpdate()
    // {
    //     JumpTriggered         = false;
    //     FireTriggered         = false;
    //     ToggleWeaponTriggered = false;
    //     RotateObjectTriggered = false;
    //     ReloadTriggered       = false;
    // }
}