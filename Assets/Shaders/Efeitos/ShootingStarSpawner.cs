using System.Collections;
using UnityEngine;

public class ShootingStarSpawner : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameObject starPrefab; 

    [Header("Configurações de Spawn")]
    [SerializeField] private float spawnInterval = 2f; 
    [SerializeField] private float spawnIntervalVariance = 1f; 
    
    [Header("Configurações da Trajetória")]
    [Tooltip("Área de spawn no eixo X (largura)")]
    [SerializeField] private Vector2 spawnAreaX = new Vector2(-10f, 10f);
    [Tooltip("Altura de spawn no eixo Y")]
    [SerializeField] private float spawnHeightY = 10f;

    [Tooltip("Distância que a estrela percorre para baixo")]
    [SerializeField] private float fallDistance = 15f;
    [Tooltip("O quanto a curva vai para a esquerda ('força' do C invertido)")]
    [SerializeField] private float curveMagnitude = 8f;

    [Tooltip("Duração do percurso (min e max)")]
    [SerializeField] private Vector2 travelDuration = new Vector2(3f, 5f);


    void Start()
    {
        StartCoroutine(SpawnStarRoutine());
    }

    private IEnumerator SpawnStarRoutine()
    {

        while (true)
        {

            float waitTime = spawnInterval + Random.Range(-spawnIntervalVariance, spawnIntervalVariance);
            yield return new WaitForSeconds(waitTime);

        
            float randomX = Random.Range(spawnAreaX.x, spawnAreaX.y);
           
            Vector3 startPos = new Vector3(randomX, spawnHeightY, 0);

           
            Vector3 endPos = new Vector3(randomX, spawnHeightY - fallDistance, 0);
            
            Vector3 controlPos = new Vector3(randomX - curveMagnitude, spawnHeightY - (fallDistance / 2), 0);

          
            GameObject starInstance = Instantiate(starPrefab, startPos, Quaternion.identity);

    
            StarMover mover = starInstance.GetComponent<StarMover>();
            if (mover != null)
            {
                float duration = Random.Range(travelDuration.x, travelDuration.y);
                mover.SetPath(startPos, controlPos, endPos, duration);
            }
        }
    }
}