using UnityEngine;

public class SoftBallDeformation : MonoBehaviour
{
    // ==== PARAMÈTRES VISIBLES DANS L'INSPECTOR ==========
    [Header("Paramètres de déformation")]
    [SerializeField] private float maxDeformation = 0.4f;      // Intensité maximale de la déformation (0.4 = 40%)
    [SerializeField] private float deformationSpeed = 20f;     // Vitesse à laquelle la balle s'aplatit
    [SerializeField] private float recoverySpeed = 8f;         // Vitesse à laquelle la balle reprend sa forme
    [SerializeField] private float minImpactVelocity = 1.5f;   // Vitesse minimale pour déclencher la déformation (m/s)
    

    private Rigidbody rb;                    // Référence au composant Rigidbody de la balle
    private Vector3 originalScale;           // Taille originale de la balle (sauvegardée au Start)
    private Vector3 targetScale;             // Taille cible vers laquelle on anime
    private bool isAnimating = false;        // Est-ce qu'une animation est en cours?
    private float animationProgress = 0f;    // Progression de l'animation (0 = début, 1 = fin)
    private bool isRecovering = false;       // Est-ce qu'on est en phase de récupération?

    // INITIALISATION 
    void Start()
    {
        // Récupérer le composant Rigidbody attaché à cet objet
        rb = GetComponent<Rigidbody>();
        
        // debuggage
        if (rb == null)
        {
            Debug.LogError(" Rigidbody manquant sur " + gameObject.name);
            enabled = false;  // Désactive ce script
            return;           // Sort de la fonction Start
        }
        
        // Sauvegarder la taille originale de la balle pour pouvoir y revenir
        originalScale = transform.localScale;
        targetScale = originalScale;  // Au début, la cible est la taille originale
        
        Debug.Log("Script initialisé sur " + gameObject.name); // debuggage
    }

    // UPDATE (appelée à chaque frame
    void Update()
    {
        // Si une animation de déformation/récupération est en cours
        if (isAnimating)
        {
            // Incrémenter la progression de l'animation
            // Time.deltaTime = temps écoulé depuis la dernière frame (~0.016s à 60 FPS)
            // On multiplie par la vitesse appropriée (deformationSpeed ou recoverySpeed)
            animationProgress += Time.deltaTime * (isRecovering ? recoverySpeed : deformationSpeed);
            
            // Si l'animation est terminée (progression >= 1 = 100%)
            if (animationProgress >= 1f)
            {
                animationProgress = 0f;  // Réinitialiser pour la prochaine animation
                
                if (isRecovering)
                {
                    // Fin de la récupération : on revient exactement à la taille originale
                    transform.localScale = originalScale;
                    isAnimating = false;   // Arrêter l'animation
                    isRecovering = false;  // On n'est plus en récupération
                }
                else
                {
                    // Fin de la déformation : commencer la phase de récupération
                    isRecovering = true;
                    targetScale = originalScale;  // La nouvelle cible est la taille originale
                }
            }
            
            // Interpolation linéaire (Lerp) pour animer progressivement la taille
            // Lerp(A, B, t) = A + (B - A) * t
            // Si t=0 → résultat=A, si t=1 → résultat=B, si t=0.5 → résultat=milieu entre A et B
            transform.localScale = Vector3.Lerp(
                isRecovering ? transform.localScale : originalScale,  // Point de départ
                targetScale,                                           // Point d'arrivée
                animationProgress                                      // Progression (0 à 1)
            );
        }
    }

    // DÉTECTION DE COLLISION 
    void OnCollisionEnter(Collision collision)
    {
        // Debugagee afficher avec quel objet on est entré en collision
        Debug.Log("🔵 Collision détectée avec: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");
        
        // Variables pour stocker les informations de collision avec le sol
        bool foundGround = false;              // A-t-on trouvé le sol?
        ContactPoint groundContact = default;  // Point de contact avec le sol
        
        // Parcourir tous les points de contact de la collision
        foreach (ContactPoint contact in collision.contacts)
        {
            // Vérifier si l'objet touché a le tag "Ground"
            if (collision.gameObject.CompareTag("Ground"))
            {
                foundGround = true;
                groundContact = contact;  // Sauvegarder le point de contact
                break;                    // Sortir de la boucle, on a trouvé ce qu'on cherchait
            }
        }
        
        // Si on a trouvé le sol
        if (foundGround)
        {
            // Récupérer la normale du contact (vecteur perpendiculaire à la surface)
            Vector3 impactNormal = groundContact.normal;
            
            // Récupérer la vélocité (vitesse) de la balle au moment de l'impact
            // magnitude = longueur du vecteur vitesse = vitesse totale en m/s
            float impactVelocity = rb.linearVelocity.magnitude;
            
            Debug.Log($"Impact sur Ground! Vélocité: {impactVelocity:F2} m/s");
            
            // Vérifier si l'impact est assez fort pour causer une déformation
            if (impactVelocity >= minImpactVelocity)
            {
                DeformOnImpact(impactVelocity, impactNormal);
            }
            else
            {
                // Impact trop faible, pas de déformation
                Debug.Log($"Impact trop faible ({impactVelocity:F2} < {minImpactVelocity})");
            }
        }
        else
        {
            Debug.Log("Pas de tag 'Ground' détecté!");
        }
    }

    // ========== CALCUL ET APPLICATION DE LA DÉFORMATION ==========
    void DeformOnImpact(float velocity, Vector3 normal)
    {
        // Calculer l'intensité de la déformation basée sur la vélocité
        // Mathf.Clamp01() limite la valeur entre 0 et 1
        // velocity / 10f normalise la vélocité (10 m/s = intensité maximale)
        // Puis on multiplie par maxDeformation pour obtenir l'intensité finale
        float deformIntensity = Mathf.Clamp01(velocity / 10f) * maxDeformation;
        
        // Calculer la nouvelle échelle (taille) de la balle déformée
        // La balle s'aplatit verticalement (Y réduit) et s'élargit horizontalement (X et Z augmentés)
        targetScale = new Vector3(
            originalScale.x * (1f + deformIntensity * 0.8f),   // X augmente de 80% de l'intensité
            originalScale.y * (1f - deformIntensity * 1.2f),   // Y diminue de 120% de l'intensité (aplatissement)
            originalScale.z * (1f + deformIntensity * 0.8f)    // Z augmente de 80% de l'intensité
        );
        
        // Exemple de calcul avec deformIntensity = 0.3 (30%) :
        // Si originalScale = (1, 1, 1)
        // X = 1 * (1 + 0.3 * 0.8) = 1 * 1.24 = 1.24 (élargi de 24%)
        // Y = 1 * (1 - 0.3 * 1.2) = 1 * 0.64 = 0.64 (aplati de 36%)
        // Z = 1 * (1 + 0.3 * 0.8) = 1 * 1.24 = 1.24 (élargi de 24%)
        
        // Réinitialiser les variables d'animation pour démarrer une nouvelle déformation
        animationProgress = 0f;       // Remettre la progression à 0
        isAnimating = true;           // Activer l'animation
        isRecovering = false;         // On commence par se déformer, pas récupérer
        
        Debug.Log($"✨ Déformation appliquée! Intensité: {deformIntensity:F2}, Échelle cible: {targetScale}");
    }
}