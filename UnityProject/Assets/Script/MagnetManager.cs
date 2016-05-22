using UnityEngine;
using System.Collections.Generic;

public class MagnetManager : MonoBehaviour 
{
	public GameObject m_MagnetPrefab = null ;
	public List<GameObject> m_Magnets = new List<GameObject>() ;
	public GameObject m_VirtualMagnet = null ;
	public List<GameObject> m_PotentialVirtualMagnets = new List<GameObject>() ;
	public List<RotateToVec> m_TargetMagnetRotates = new List<RotateToVec>() ;
	public GameObject m_TargetParent = null ;
	public GameObject m_VirtualParent = null ;
	
	
	public Vector3 m_VirtualMagnetPosition = Vector3.zero ;
	public Vector3 m_VirtualMagnetForward = Vector3.zero ;
	
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
		
		
		dotFromNorthPole = Mathf.Abs( dotFromNorthPole )  ;
		
		float ratioOfN = 0.0f ;
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
		
//		Debug.Log( "dotFromNorthPole=" + dotFromNorthPole );
//		Debug.Log( "ratioOfN=" + ratioOfN );
//		Debug.Log( "tangentVec=" + tangentVec );
//		Debug.Log( "toN=" + toN );
//		Debug.Log( "combinedTangent=" + combinedTangent );

		Quaternion ret = Quaternion.LookRotation( combinedTangent , upVec ) ;
		return ret ;
	}

	// Use this for initialization
	void Start () 
	{
		GenerateRandomTargets() ;
		
		CollectTargets() ;
		
		CollectPresetVirtuals() ;
		
		GenerateVirtualMagnet() ;

		
		
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
			for( int i = 0 ; i < m_TargetMagnetRotates.Count ; ++i )
			{
				if( null != m_TargetMagnetRotates[ i ] )
				{
					m_TargetMagnetRotates[ i ].m_ReferenceTransform = m_VirtualMagnet.transform ;
					m_TargetMagnetRotates[ i ].CalculateTargetPose() ;
				}			
			}

		}
	}
	
	void GenerateRandomTargets()
	{
		int generatingNum = 10 ;
		float size = 5.0f ;
	
		Vector3 randomPos = Vector3.zero ;
		
		for( int i = 0 ; i < generatingNum ; ++i )
		{
			GameObject addObj = (GameObject) GameObject.Instantiate( m_MagnetPrefab ) ;
			if( null != addObj )
			{
				addObj.name = "magnet" + i.ToString() ;
				randomPos = Random.onUnitSphere ;
				randomPos *= size ;
				randomPos.z = 0 ;
				addObj.transform.position = randomPos ;
				addObj.transform.parent = m_TargetParent.transform ;
			}		
		}

	}
	
	void CollectTargets()
	{
		Transform trans = null ;
		for( int i = 0 ; i < m_TargetParent.transform.childCount ; ++i )
		{
			trans = m_TargetParent.transform.GetChild( i ) ;
			if( null != trans )
			{
				RotateToVec rotate = trans.gameObject.GetComponent<RotateToVec>() ;
				if( null != rotate )
				{
					m_TargetMagnetRotates.Add( rotate ) ;
				}
			}
		}
	}
	
	void CollectPresetVirtuals()
	{
		Transform trans = null ;
		for( int i = 0 ; i < m_VirtualParent.transform.childCount ; ++i )
		{
			trans = m_VirtualParent.transform.GetChild( i ) ;
			if( null != trans )
			{
				m_PotentialVirtualMagnets.Add( trans.gameObject ) ;
			}
		}
	}

	void GenerateVirtualMagnet()
	{
		if( null != m_MagnetPrefab )
		{
			CalculateVirtualMagnetPose () ;
		
			m_VirtualMagnet = new GameObject() ;
			m_VirtualMagnet.name = "VirtualMagnet" ;
			m_VirtualMagnet.transform.position = m_VirtualMagnetPosition ;
			m_VirtualMagnet.transform.rotation = Quaternion.LookRotation( m_VirtualMagnetForward ) ;
		}
	}	
	
	void CalculateVirtualMagnetPose ()
	{
		Vector3 sumVec = Vector3.zero ;
		Vector3 directionSum = Vector3.zero ;
		int count = m_PotentialVirtualMagnets.Count ;
		foreach( GameObject obj in m_PotentialVirtualMagnets )
		{
			sumVec += obj.transform.position ;
			directionSum += obj.transform.forward ;
		}
		
		sumVec *= ( 1.0f / count ) ;
		m_VirtualMagnetPosition = sumVec ;
		directionSum *= ( 1.0f / count ) ;
		m_VirtualMagnetForward = directionSum ;
		
	}
}
