﻿using UnityEngine;
using System.Collections.Generic;

public class RelationHub
{
	public GameObject Me = null ;
	public List<GameObject> Friends = new List<GameObject>() ;
}

public class MagnetManager : MonoBehaviour 
{
	public GameObject m_MagnetPrefab = null ;
	public List<GameObject> m_Magnets = new List<GameObject>() ;
	public GameObject m_VirtualMagnet = null ;
	public List<GameObject> m_PotentialVirtualMagnets = new List<GameObject>() ;
	public List<RotateToVec> m_TargetMagnetRotates = new List<RotateToVec>() ;
	public GameObject m_TargetParent = null ;
	public GameObject m_VirtualParent = null ;
	public Material m_MagnetMaterialBoth = null ;
	public Material m_MagnetMaterialNorth = null ;
	public Material m_MagnetMaterialSouth = null ;
	
	public Vector3 m_VirtualMagnetPosition = Vector3.zero ;
	public Vector3 m_VirtualMagnetForward = Vector3.zero ;
	
	private float m_VirtualMagnetMaximumDistance = 2.0f ;
	
	bool m_IsModifing = false ;
	bool m_IsCheckingVirtualMagnetMoving = false ;
	
	public void CalculateVirtualMagnet()
	{
		// scan all in target and collect those groups
		List<RelationHub> relations = new List<RelationHub>() ;
		for( int i = 0 ; i < m_TargetMagnetRotates.Count ; ++i )
		{
			GameObject me = m_TargetMagnetRotates[ i ].gameObject ;
			RelationHub relation = null ;
			for( int j = 0 ; j < m_TargetMagnetRotates.Count ; ++j )
			{
				if( j == i )
					continue ;
					
				GameObject her = m_TargetMagnetRotates[ j ].gameObject ;	
				float distance = Vector3.Distance( me.transform.position , her.transform.position ) ;
				if( distance < m_VirtualMagnetMaximumDistance )
				{
					if( null == relation )
					{
						relation = new RelationHub() ;
						relation.Me = me ;
					}
					
					relation.Friends.Add( her ) ;
					
				}
			}
			if( null != relation )
			{
				// Debug.Log("relation.Me.name=" + relation.Me.name );
				// Debug.Log("relation.Friends.Count=" + relation.Friends.Count );
				relations.Add( relation ) ;
			}
		}
		
		// Debug.Log("relations.Count=" + relations.Count );
		
		// find out the largest group.
		
		int maxFriends = 0 ;
		int maxIndex = -1 ;
		for( int k = 0 ; k < relations.Count ; ++k )
		{
			if( relations[ k ].Friends.Count > maxFriends )
			{
				maxFriends = relations[ k ].Friends.Count ;
				maxIndex = k ;
			}
		}
		
		if( -1 == maxIndex )
		{
			Debug.LogWarning("There is no such biggest group");
			return ;
		}
		
		// move the largest group from m_TargetMagnetRotates 
		// into m_PotentialVirtualMagnets
		// and then they can be calculated into virtual magnet.
		RelationHub maxGroup = relations[ maxIndex ] ;
		maxGroup.Me.gameObject.transform.parent = m_VirtualParent.transform ;
		for( int m = 0 ; m < maxGroup.Friends.Count ; ++m )
		{
			maxGroup.Friends[m].transform.parent = m_VirtualParent.transform ;
		}
		
		
		
		/**
		Collect preset target in m_TargetParent to m_TargetMagnetRotates
		2nd time
		*/
		CollectTargets() ;
		
		/**
		Collect preset virtual in m_VirtualParent to m_PotentialVirtualMagnets
		2nd time
		*/
		CollectPresetVirtuals() ;
		
		
		/**
		by using m_PotentialVirtualMagnets to generate a virtual magnet.
		*/
		GenerateVirtualMagnet() ;
		
		RefineMagnetAsVirtualMagnet() ;
		
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
		GenerateRandomTargets() ;// generate to m_TargetParent
		
		
		
		
		/**
		Collect preset target in m_TargetParent to m_TargetMagnetRotates
		1st time.
		*/
		CollectTargets() ;
		
		/**
		Collect preset virtual in m_VirtualParent to m_PotentialVirtualMagnets
		1st time.
		*/
		CollectPresetVirtuals() ;
		
		
		/**
		scan target to make them change into m_PotentialVirtualMagnets
		*/
		CalculateVirtualMagnet() ;
		

	}
	
	// Update is called once per frame
	void Update () 
	{
		if( false == m_IsModifing 
		)
		{
			if( true == m_IsCheckingVirtualMagnetMoving )
			{
				if( true == CheckIfVirtualMagnetIsStopped() )
				{
					StartRotateTargetMagnetRotateMagnet() ;
					m_IsCheckingVirtualMagnetMoving = false ;
				}
			}
		}
	}
	
	void StartRotateTargetMagnetRotateMagnet()
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
		
		m_TargetMagnetRotates.Clear() ;
	}
	
	void GenerateRandomTargets()
	{
		if( null == m_MagnetPrefab )
		{
			return ;
		}
		
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
		m_TargetMagnetRotates.Clear() ;
		Transform trans = null ;
		for( int i = 0 ; i < m_TargetParent.transform.childCount ; ++i )
		{
			trans = m_TargetParent.transform.GetChild( i ) ;
			if( null != trans )
			{
				RotateToVec rotate = trans.gameObject.GetComponent<RotateToVec>() ;
				if( null == rotate )
				{
					rotate = trans.gameObject.AddComponent<RotateToVec>() ;
				}
				
				m_TargetMagnetRotates.Add( rotate ) ;
			}
		}
	}
	
	void CollectPresetVirtuals()
	{
		Transform trans = null ;
		m_PotentialVirtualMagnets.Clear() ;
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

		// calculate all object in m_PotentialVirtualMagnets
		CalculateVirtualMagnetPose () ;
		
		if( null == m_VirtualMagnet )
		{
			m_VirtualMagnet = new GameObject() ;
			m_VirtualMagnet.name = "VirtualMagnet" ;
		}
		
		m_VirtualMagnet.transform.position = m_VirtualMagnetPosition ;
		m_VirtualMagnet.transform.rotation = Quaternion.LookRotation( m_VirtualMagnetForward ) ;
	}	
	
	void CalculateVirtualMagnetPose ()
	{
		Vector3 sumVec = Vector3.zero ;
		int count = m_PotentialVirtualMagnets.Count ;
		foreach( GameObject obj in m_PotentialVirtualMagnets )
		{
			sumVec += obj.transform.position ;
		}
		
		sumVec *= ( 1.0f / count ) ;
		m_VirtualMagnetPosition = sumVec ;
		
		// find the longest object
		float maxDist = 0.0f ;
		Vector3 maxVec = Vector3.zero ;
		foreach( GameObject obj in m_PotentialVirtualMagnets )
		{
			Vector3 distVec = obj.transform.position - sumVec ;
			float sMag = distVec.sqrMagnitude ;
			if( sMag > maxDist )
			{
				maxDist = sMag ;
				maxVec = distVec ;
			}
		}
		
		maxVec.Normalize() ;
		
		m_VirtualMagnetForward = maxVec ;
		// Debug.LogWarning("m_VirtualMagnetForward" + m_VirtualMagnetForward);
		
	}
	
	private void RefineMagnetAsVirtualMagnet()
	{
		foreach( GameObject obj in m_PotentialVirtualMagnets )
		{
			obj.transform.rotation = m_VirtualMagnet.transform.rotation ;
		}
		
		Vector3 virtualMagnetPos = m_VirtualMagnet.transform.position ;
		float num = m_PotentialVirtualMagnets.Count ;
		// modify their position to let then stick together
		for( int i = 0 ; i < num; ++i )
		{
			MoveToPos moveToPos = m_PotentialVirtualMagnets[ i ].AddComponent<MoveToPos>() ;
			moveToPos.m_DestinationPos = virtualMagnetPos + m_VirtualMagnetForward * (i - (num-1)*0.5f)  ;
		}
		
		ModifyMaterialForPotentialVirtualMagnets() ;
		
		m_IsCheckingVirtualMagnetMoving = true ;
	}
	
	private void ModifyMaterialForPotentialVirtualMagnets() 
	{
		Vector3 virtualMagnetCenter = m_VirtualMagnetPosition ;
		Vector3 virtualMagnetDirection = m_VirtualMagnetForward ;
		Vector3 eachPos ;
		Vector3 toEachVec ;
		float dotResult = 0.0f ;
		float secondDotResult = 0.0f ;
		Renderer[] renderers = null ;
		
		
		for( int i = 0 ; i < m_PotentialVirtualMagnets.Count ; ++i )
		{
			MoveToPos moveToPos = m_PotentialVirtualMagnets[ i ].GetComponent<MoveToPos>() ;
			eachPos = moveToPos.m_DestinationPos ;
			toEachVec = eachPos - virtualMagnetCenter ;
			toEachVec.Normalize() ;
			dotResult = Vector3.Dot (toEachVec,virtualMagnetDirection );
			renderers = m_PotentialVirtualMagnets[i].GetComponentsInChildren<Renderer>() ;
			foreach( Renderer renderer in renderers ) 
			{

				if(dotResult>0.0f)
				{
					renderer.material = m_MagnetMaterialNorth ;
				}
				else if(dotResult<0.0f)
				{
					renderer.material = m_MagnetMaterialSouth ;
					
				}
				else
				{
					Vector3 localVec = renderer.gameObject.transform.position - m_PotentialVirtualMagnets[ i ].transform.position ;
					toEachVec = eachPos + localVec - virtualMagnetCenter ;
					toEachVec.Normalize() ;
					secondDotResult = Vector3.Dot (toEachVec,virtualMagnetDirection );
					
					// Debug.Log("eachPos" + eachPos);
					// Debug.Log("toEachVec" + toEachVec);
					// Debug.Log("secondDotResult" + secondDotResult);
					
					if(secondDotResult>0.0f)
					{
						renderer.material = m_MagnetMaterialNorth ;
					}
					else if(secondDotResult<0.0f)
					{
						renderer.material = m_MagnetMaterialSouth ;
					}
					else
					{
						renderer.material = m_MagnetMaterialBoth ;
					}
				}
				
			}
		}	
	}
	
	bool CheckIfVirtualMagnetIsStopped()
	{
		bool ret = false ;
		GameObject parent = null ;
		foreach( GameObject obj in m_PotentialVirtualMagnets ) 
		{
			if( null != obj.transform.parent )
			{
				parent = obj.transform.parent.gameObject ;
			}
			break ;
		}
		
		if( null != parent )
		{
			MoveToPos [] moveToPosVec = parent.GetComponentsInChildren<MoveToPos>() ;
			
			ret = ( moveToPosVec.Length <= 0 ) ;
		}
		
		return ret ;
	}
}


