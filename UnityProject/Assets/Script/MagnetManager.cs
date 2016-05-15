using UnityEngine;
using System.Collections.Generic;

public class MagnetManager : MonoBehaviour 
{
	public GameObject m_MagnetPrefab = null ;
	public List<GameObject> m_Magnets = new List<GameObject>() ;
	public GameObject m_VirtualMagnet = null ;
	public GameObject m_TargetMagnets = null ;
	
	bool m_IsModied = false ;
	
	public void CalculateVirtualMagnet()
	{
	
	}
	
	public static Quaternion CalculateMagnetDirection( Transform _ReferenctPos , Vector3 _TargetPosition )
	{
		Vector3 vecToTarget = _TargetPosition - _ReferenctPos.position ;
		vecToTarget.Normalize() ;
		// dot = reference forward * vecToTarget
		// will use dotFromNorthPole as a ratio
		float dotFromNorthPole = Vector3.Dot ( _ReferenctPos.forward , vecToTarget ) ;
		
		float ratioOfN = 0.0f ;
		if( dotFromNorthPole > 0.0f )
		{
			
		}
		else
		{
			dotFromNorthPole = -1 * dotFromNorthPole ;
		}
		
		ratioOfN = Mathf.Clamp( dotFromNorthPole , 0.9f , 1.0f ) ;
		ratioOfN = (dotFromNorthPole - 0.9f) * 10.0f ;
		
		// Force A. line magnet from N 
		Vector3 toN = _ReferenctPos.forward ;
		
		// Force B. sphere magnet between target and reference.
		Vector3 center = _TargetPosition + _ReferenctPos.position ;
		center *= 0.5f ;
		Vector3 upVec = vecToTarget ;
		// the magnet pose of Force B is a vector combined of reverse of forward with upVec.
		
		Vector3 toS = toN * -1.0f ;
		Vector3 tangentVec = Vector3.ProjectOnPlane( toS , upVec ) ;
		
		// combine 
		Vector3 combinedTangent = tangentVec * ( 1.0f - ratioOfN ) + toN * ratioOfN ;
		
		Debug.Log( "dotFromNorthPole=" + dotFromNorthPole );
		Debug.Log( "ratioOfN=" + ratioOfN );
		Debug.Log( "tangentVec=" + tangentVec );
		Debug.Log( "toN=" + toN );
		Debug.Log( "combinedTangent=" + combinedTangent );
		
		Quaternion ret = Quaternion.LookRotation( combinedTangent , upVec ) ;
		return ret ;
	}

	// Use this for initialization
	void Start () 
	{
		if( null != m_MagnetPrefab )
		{
			GameObject addObj = (GameObject) GameObject.Instantiate( m_MagnetPrefab ) ;
			if( null != addObj )
			{
				addObj.name = "magnet" ;
				m_VirtualMagnet = addObj ;
			}
		}

	}
	
	// Update is called once per frame
	void Update () 
	{
		if( false == m_IsModied )
		{
			UpdateMagnet() ;
		}
	}
	
	void UpdateMagnet()
	{
		if( null != m_VirtualMagnet )
		{
			RotateToVec rotate = m_TargetMagnets.GetComponent<RotateToVec>() ;
			if( null != rotate )
			{
				rotate.m_ActiveRotate = true ;
				rotate.m_ReferenceTransform = m_VirtualMagnet.transform ;
			}
		}
	}
	
}
