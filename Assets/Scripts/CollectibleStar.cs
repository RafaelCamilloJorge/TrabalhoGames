using System.Collections.Generic;
using UnityEngine;

public class CollectibleStar : MonoBehaviour
{
    static readonly List<CollectibleStar> Registry = new List<CollectibleStar>();
    [Header("Config")]
    [SerializeField] int value = 1;
    [SerializeField] float spinSpeed = 90f;
    [SerializeField] AudioClip sfxPickup;
    [SerializeField] ParticleSystem vfxPickup;

    bool _collected;

    void Awake()
    {
        Registry.Add(this);
    }

    void OnDestroy()
    {
        Registry.Remove(this);
    }

    void OnEnable()
    {
        _collected = false;
    }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player")) return;

        _collected = true;

        if (sfxPickup) AudioSource.PlayClipAtPoint(sfxPickup, transform.position);
        if (vfxPickup) Instantiate(vfxPickup, transform.position, Quaternion.identity);

        ScoreService.Add(value);

        gameObject.SetActive(false);
    }

    public static void ResetAll()
    {
        for (int i = 0; i < Registry.Count; i++)
        {
            var star = Registry[i];
            if (star != null && !star.gameObject.activeSelf)
                star.gameObject.SetActive(true);
        }
    }

    public static int TotalPossibleScore
    {
        get
        {
            int total = 0;
            for (int i = 0; i < Registry.Count; i++)
            {
                var star = Registry[i];
                if (star != null)
                    total += star.value;
            }
            return total;
        }
    }
}
