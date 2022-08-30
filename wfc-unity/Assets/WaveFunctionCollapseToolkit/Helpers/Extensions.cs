using System.Collections;
using UnityEngine;

public static class Extensions {
    public static int Sample(this float[] weights) {
        float sum = 0;
        for(int i = 0; i < weights.Length; i++)
            sum += weights[i];
        float threshold = Random.Range(0f, 1f) * sum;

        float partialSum = 0;
        for(int i = 0; i < weights.Length; i++) {
            partialSum += weights[i];
            if(partialSum >= threshold)
                return i;
        }
        return 0;
    }

    public class CoroutineRunner : MonoBehaviour { }
    static CoroutineRunner _coroutineRunner;
    static CoroutineRunner coroutineRunner {
        get {
            if(_coroutineRunner == null)
                _coroutineRunner = new GameObject("CoroutineRunner").AddComponent<CoroutineRunner>();
            return _coroutineRunner;
        }
    }

    public static Coroutine RunCoroutine(this IEnumerator item) {
        return coroutineRunner.StartCoroutine(item);
    }

    public static void StopCoroutine(this Coroutine coroutine) {
        coroutineRunner.StopCoroutine(coroutine);
    }
}