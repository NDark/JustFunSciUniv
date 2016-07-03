using UnityEngine;
using System.Collections.Generic;

public enum MagnetManagerState
{
	Invalid = 0 ,
	Initializing ,
	Calculating ,
	WaitingVirtualMagnetAnimation ,
	WaitingTargetMagnetAnimation ,
	Valid ,
	WaitingInput ,
}

public class MagnetManager : MonoBehaviour 
{
	public GameObject m_MagnetPrefab = null ;
	public List<GameObject> m_Magnets = new List<GameObject>() ;
	public GameObject m_VirtualMagnet = null ;
	public List<GameObject> m_PotentialVirtualMagnets = new List<GameObject>() ;
	public List<GameObject> m_TargetMagnets = new List<GameObject>();
	public GameObject m_TargetParent = null ;
	public GameObject m_VirtualParent = null ;
	public Material m_MagnetMaterialBoth = null ;
	public Material m_MagnetMaterialNorth = null ;
	public Material m_MagnetMaterialSouth = null ;
	
	public Vector3 m_VirtualMagnetPosition = Vector3.zero ;
	public Vector3 m_VirtualMagnetForward = Vector3.zero ;
	
	public MagnetManagerState m_State = MagnetManagerState.Invalid ;
	
	private float m_VirtualMagnetMaximumDistance = 2.0f ;
	
	protected void CalculateAndDecideVirtuaMagnet()
	{
		/**
		From m_PotentialVirtualMagnets and m_TargetMagnets to collect allMagnets.
		
		For all group , collect possible neighbor arround this group.
		Each test object shall be near to one member of this group.
		
		Find the largest group, and make it its children be the virtual parent.
				
		Result:
		
		Make those virtual magnet to the children of m_VirtualParent.
		*/
		
		List<GameObject> allMagnets = new List<GameObject>() ;
		foreach( GameObject obj in m_PotentialVirtualMagnets )
		{
			allMagnets.Add( obj ) ;
		}
		foreach( GameObject obj in m_TargetMagnets )
		{
			allMagnets.Add( obj ) ;
		}
		
		// create the first group.
		List<RelationHub> relations = new List<RelationHub>() ;
		RelationHub firstGroup = null ;
		foreach( GameObject obj in m_PotentialVirtualMagnets )
		{
			if( null == firstGroup )
			{
				firstGroup = new RelationHub() ;
				firstGroup.Me = obj ;
				firstGroup.Friends.Add( obj ) ;
				relations.Add( firstGroup ) ;
				
			}
			else
			{
				firstGroup.Friends.Add( obj ) ;
			}
			
		}
		
		Debug.Log("relations.Count=" + relations.Count );

		// ignore those exist in first group
		if( null != firstGroup )
		{
			for( int i = 0 ; i < allMagnets.Count ; ++i )
			{
				if( firstGroup.IsInclude( allMagnets[ i ] ) )
				{
					allMagnets[ i ] = null ;
				}
				
			}				
		}
		
		// create a first search
		for( int i = 0 ; i < allMagnets.Count ; ++i )
		{
			if( null == allMagnets[ i ] )
			{
				continue ;
			}
			
			GameObject me = allMagnets[ i ] ;
			RelationHub relation = null ;
			
			for( int j = i+1 ; j < allMagnets.Count ; ++j )
			{
				if( null == allMagnets[ j ] )
				{
					continue ;
				}
				
				GameObject her = allMagnets[ j ] ;	
				float distance = Vector3.Distance( me.transform.position 
					, her.transform.position ) ;
				if( distance < m_VirtualMagnetMaximumDistance )
				{
					if( null == relation )
					{
						relation = new RelationHub() ;
						relation.Me = me ;
						relation.Friends.Add( me ) ;
						// Debug.LogWarning("relations.Add" );		
						relations.Add( relation ) ;
						
					}
					relation.Friends.Add( her ) ;
				}
			}
		}
		
		Debug.LogWarning("relations.Count=" + relations.Count );
		
		GraduatelyMergeRelations( relations ) ;
		
		Debug.LogWarning("relations.Count=" + relations.Count );
		
		// find out the largest group.
		
		int maxFriends = 0 ;
		int maxIndex = -1 ;
		for( int k = 0 ; k < relations.Count ; ++k )
		{
			if( null == relations[ k ])
			{
				continue ;
			}
			
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
		
		// move the largest group from m_TargetMagnets 
		// into m_PotentialVirtualMagnets
		// and then they can be calculated into virtual magnet.
		RelationHub maxGroup = relations[ maxIndex ] ;
		maxGroup.Me.gameObject.transform.parent = m_VirtualParent.transform ;
		for( int m = 0 ; m < maxGroup.Friends.Count ; ++m )
		{
			maxGroup.Friends[m].transform.parent = m_VirtualParent.transform ;
		}
		
	}
	
	protected void GraduatelyMergeRelations( List<RelationHub> _Groups )
	{
		bool touched = true ;
		int count = 0 ;
		int maxCount = 10 ;
		while( touched && count < maxCount )
		{
			touched = false ;
			// Debug.Log("count" + count );
			++count ;
			
			for( int i = 0 ; i < _Groups.Count ; ++i )
			{
				if( null == _Groups [ i ])
				{
					continue ;
				}

				float tempDistance = 0.0f ;				
				int minIndex = -1 ;
				float minDistance = 999.99f ;
				for( int j = i + 1 ; j < _Groups.Count ; ++j )
				{
					if( null == _Groups [ j ])
					{
						continue ;
					}
					
					tempDistance = _Groups[i].CalculateNearestDistance( _Groups[j] ) ;
					if( tempDistance < m_VirtualMagnetMaximumDistance )
					{
						if( tempDistance < minDistance )
						{
							minDistance = tempDistance ;
							minIndex = j ;
						}
					}
				}
				
				if( minIndex != -1 )
				{
					touched = true ;
					_Groups[ i ].MergeGroup( _Groups[ minIndex ] ) ;
					Debug.Log("_Groups[ i ].MergeGroup minIndex=" + minIndex ) ;
					_Groups[ minIndex ] = null ;
				}
			}
		}
	}
	
	public void CalculateVirtualMagnet()
	{
		CalculateAndDecideVirtuaMagnet() ;

		
		
		
		/**
		Collect preset target in m_TargetParent to m_TargetMagnets
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

		

	}
	
	// Update is called once per frame
	void Update () 
	{
		switch( m_State )
		{
		case MagnetManagerState.Invalid :
			m_State = MagnetManagerState.Initializing ;
			break ;
		case MagnetManagerState.Initializing :
			Flow_MagnetManagerStateInitializing() ;
			m_State = MagnetManagerState.Calculating ;
			break ;
		case MagnetManagerState.Calculating :
			Flow_MagnetManagerStateCalculating() ;
			m_State = MagnetManagerState.WaitingVirtualMagnetAnimation ;
			break ;
		case MagnetManagerState.WaitingVirtualMagnetAnimation :
			Flow_MagnetManagerStateWaitingVirtualMagnetAnimation() ;
			break ;
		case MagnetManagerState.WaitingTargetMagnetAnimation :
			break ;
		case MagnetManagerState.Valid :
			break ;
		case MagnetManagerState.WaitingInput :
			break ;
			
		}
		
		
	}
	
	void StartRotateTargetMagnetRotateMagnet()
	{
		if( null != m_VirtualMagnet )
		{
			for( int i = 0 ; i < m_TargetMagnets.Count ; ++i )
			{
				if( null != m_TargetMagnets[ i ] )
				{
					RotateToVec r = m_TargetMagnets[ i ].AddComponent<RotateToVec>() ;
					if( null != r )
					{
						r.m_ReferenceTransform = m_VirtualMagnet.transform ;
						r.CalculateTargetPose() ;
					}
				}			
			}
		}
		
	}
	

	void CollectTargets()
	{
		m_TargetMagnets.Clear() ;
		Transform trans = null ;
		for( int i = 0 ; i < m_TargetParent.transform.childCount ; ++i )
		{
			trans = m_TargetParent.transform.GetChild( i ) ;
			if( null != trans )
			{
				m_TargetMagnets.Add( trans.gameObject ) ;
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
		
		if( 0 == count )
		{
			Debug.Log("0 == count");
			return ;
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
	
	void Flow_MagnetManagerStateInitializing() 
	{
	
		GenerateRandomTargets() ;// generate to m_TargetParent
		
		
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

	void Flow_MagnetManagerStateCalculating()
	{
		
		
		
		/**
		Collect preset target in m_TargetParent to m_TargetMagnets
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
	
	void Flow_MagnetManagerStateWaitingVirtualMagnetAnimation()
	{
		if( true == CheckIfVirtualMagnetIsStopped() )
		{
			StartRotateTargetMagnetRotateMagnet() ;
			m_State = MagnetManagerState.WaitingTargetMagnetAnimation ;
		}	
	}
}


