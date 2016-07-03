using UnityEngine;
using System.Collections.Generic;

public class RelationHub
{
	public GameObject Me = null ;
	public List<GameObject> Friends = new List<GameObject>() ;
	
	public bool IsInclude( GameObject _TestObj )
	{
		
		for( int i = 0 ; i < this.Friends.Count ; ++i )
		{
			if( _TestObj == this.Friends[ i ] )
			{
				return true ;
			}
		}
		
		return false ;
	}
	
	public bool IsNear( GameObject _TestObj , float _TestLimit )
	{
		float dist = 0.0f ;
		
		for( int i = 0 ; i < this.Friends.Count ; ++i )
		{
			dist = Vector3.Distance( _TestObj.transform.position 
			                        , this.Friends[ i ].transform.position ) ;
			if( dist < _TestLimit )
			{
				return true ;
			}
		}
		return false ;
	}
	
	
	public float CalculateNearestDistance( RelationHub _TestGroup )
	{
		float minDist = float.MaxValue ;
		float dist = 0.0f ;
		
		for( int i = 0 ; i < this.Friends.Count ; ++i )
		{
			for( int j = 0 ; j < _TestGroup.Friends.Count ; ++j )
			{
				dist = Vector3.Distance( _TestGroup.Friends[ j ].transform.position 
				                        , this.Friends[ i ].transform.position ) ;
				if( dist < minDist )
				{
					minDist = dist ;
				}
			}
		}
		
		return minDist ;
	}
	
	
	public void MergeGroup( RelationHub _MergedGroup )
	{
		for( int i = 0 ; i < _MergedGroup.Friends.Count ; ++i )
		{
			this.Friends.Add( _MergedGroup.Friends[ i ] ) ;
		}
	}
}
