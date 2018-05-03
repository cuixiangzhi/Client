//using UnityEngine;
//using System.Collections;

//public class RendMega : MonoBehaviour {
//	public GameObject m_kPath	   = null;

//    public float midHeight = 2;
//    public bool isAutoHeight = false;

//    //bool beenRender = false;
// 	// Use this for initialization
//	void Start () {

//	}
	
//	public void RenderPath(Vector3 startPos, Vector3 endPos)
//	{
//        //if (beenRender == false)
//        //{
//        //    beenRender = true;
//        //    this.gameObject.SetActive(true);
//        //}
        
//        Vector3 v3FromPos;
//        Vector3 v3TargetPos;

//        this.transform.position = startPos;
//        v3FromPos = endPos - startPos;
//        v3TargetPos = Vector3.zero;

        
//		Vector3 v3MidPos    = (v3FromPos + v3TargetPos)/2;

//        MegaShapeArc msa = m_kPath.GetComponent<MegaShapeArc>();

//        if (isAutoHeight)
//        {
//            float height = msa.GetCurveLength(0) / 10 * midHeight;
//            v3MidPos += new Vector3(0, height, 0);
//        }
//        else
//        {
//            v3MidPos += new Vector3(0, midHeight, 0);
//        }
        

//		Vector3 V3Midvect = (v3TargetPos - v3FromPos)/4;


//		Vector3 v3FromAnchorIn   = 	v3FromPos+new Vector3(0,-(v3MidPos.y/3),0) ;
//		Vector3 v3FromAnchorOut  = 	v3FromPos+new Vector3(0,(v3MidPos.y/3),0)  ;

//		Vector3 v3TarGetAnchorIn  = v3TargetPos +new Vector3(0,(v3MidPos.y/3),0) ;
//		Vector3 v3TarGetAnchorOut = v3TargetPos +new Vector3(0,-(v3MidPos.y/3),0) ;

//		Vector3 v3MidAnchorIn 	  =	v3MidPos - V3Midvect;
//		Vector3 v3MidAnchorOut 	  =	v3MidPos + V3Midvect ;


        
//        msa.SetKnotEx(0, 0, v3FromPos, v3FromAnchorIn, v3FromAnchorOut);
//        msa.SetKnotEx(0, 1, v3MidPos, v3MidAnchorIn, v3MidAnchorOut);
//        msa.SetKnotEx(0, 2, v3TargetPos, v3TarGetAnchorIn, v3TarGetAnchorOut);

//	}




//}
