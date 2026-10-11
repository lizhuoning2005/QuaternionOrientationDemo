using UnityEngine;

public class QuaternionDemo : MonoBehaviour
{
    [Header("关键帧姿态（欧拉角）")]
    public Vector3 startEuler;
    public Vector3 endEuler;

    public enum InterpMode
    {
        EulerLerp,
        QuatLerpNormalize,
        QuatSlerp_Builtin,
        QuatSlerp_Manual 
    }
    [Header("插值模式选择")]
    public InterpMode mode;

    [Header("动画控制")]
    [Range(0f, 1f)] public float t;    //插值系数t∈[0,1]
    public bool autoPlay = true;       //自动往复播放动画
    public float speed = 0.1f;

    private Quaternion qStart;
    private Quaternion qEnd;

    void Update()
    {
        qStart = Quaternion.Euler(startEuler);
        qEnd = Quaternion.Euler(endEuler);

        if (autoPlay)
        {
            t = Mathf.PingPong(Time.time * speed, 1.0f);
        }

        Quaternion currentRot;
        switch (mode)
        {
            case InterpMode.EulerLerp:
            {
                Vector3 curEuler = Vector3.Lerp(startEuler, endEuler, t);
                Quaternion rx = Quaternion.Euler(curEuler.x,0,0);
                Quaternion ry = Quaternion.Euler(0,curEuler.y,0);
                Quaternion rz = Quaternion.Euler(0,0,curEuler.z);
                currentRot = rz * ry * rx;
                break;
            }
            case InterpMode.QuatLerpNormalize:
            {
                Quaternion rawQ = Quaternion.Lerp(qStart, qEnd, t);
                currentRot = rawQ.normalized;
                break;
            }
            case InterpMode.QuatSlerp_Builtin:
            {
                currentRot = Quaternion.Slerp(qStart, qEnd, t);
                break;
            }
            case InterpMode.QuatSlerp_Manual:
            {
                currentRot = ManualSlerp(qStart, qEnd, t);
                break;
            }
            default:
                currentRot = qStart;
                break;
        }
        transform.rotation = currentRot;

        //调试打印，t=0.5输出数值
        if (Mathf.Abs(t - 0.5f) < 0.005f)
        {
            Debug.Log($"==== t=0.5 | Mode:{mode} ====");
            Debug.Log($"Quat: {currentRot} | EulerAngles:{currentRot.eulerAngles}");
        }
    }

    Quaternion ManualSlerp(Quaternion q1, Quaternion q2, float timeT)
    {
        float dot = Quaternion.Dot(q1, q2);
        if (dot < 0)
        {
            q2 = new Quaternion(-q2.x, -q2.y, -q2.z, -q2.w);
            dot = -dot;
        }
        dot = Mathf.Clamp01(dot);
        float phi = Mathf.Acos(dot);
        float sinPhi = Mathf.Sin(phi);
        if (sinPhi < 1e-6f)
            return q1;

        float coeff1 = Mathf.Sin(phi * (1 - timeT)) / sinPhi;
        float coeff2 = Mathf.Sin(phi * timeT) / sinPhi;

        Quaternion res = new Quaternion(
            coeff1 * q1.x + coeff2 * q2.x,
            coeff1 * q1.y + coeff2 * q2.y,
            coeff1 * q1.z + coeff2 * q2.z,
            coeff1 * q1.w + coeff2 * q2.w
        );
        return res;
    }
}
