using System.Collections.Generic;
using System.Collections;
using System;
using System.Reflection;

public class MapDataSerializerTools
{
    public class MapDataFiledInfo
    {
        public int Tag;
        public string Name;
        public PropertyInfo filedInfo;

        public MapDataFiledInfo(int tag,string name,PropertyInfo info)
        {
            Tag = tag;
            Name = name;
            filedInfo = info;
        }
    }

    public static List<MapDataFiledInfo> GetProtoFields(object obj)
    {
        List<MapDataFiledInfo> ret = new List<MapDataFiledInfo>();

        if( null == obj )
        {
            return ret;
        }

        PropertyInfo[] fields = obj.GetType().GetProperties();

        foreach(var field in fields)
        {
            object[] attrs = field.GetCustomAttributes( typeof(global::ProtoBuf.ProtoMemberAttribute) , false);

            foreach(global::ProtoBuf.ProtoMemberAttribute obj1 in attrs)
            {
                MapDataFiledInfo info = new MapDataFiledInfo(obj1.Tag, obj1.Name, field);
                ret.Add( info );
            }
        }

        return ret;
    }

    public static void AssignDataByFieldName(object source, object dest)
    {
        List<MapDataFiledInfo> sourceAttrs = GetProtoFields( source );
        List<MapDataFiledInfo> destAttrs = GetProtoFields( dest );

        if(destAttrs.Count == 0)
            return;

        for(int k = 0 ; k < destAttrs.Count ; k++)
        {
            MapDataFiledInfo destInfo  = destAttrs[k];
            MapDataFiledInfo sourceInfo =  GetMathcedProperty(sourceAttrs, destInfo);
            
            if(null == sourceInfo)
                continue;

            try
            {
                if(null == destInfo.filedInfo.GetValue(dest, null))
                {
    				if(null == sourceInfo.filedInfo.GetValue(source, null))
    					continue;

    				object val = null;
    				if(IsStringType(sourceInfo.filedInfo))
    				{
    					char[] lpCh = {};
    					object[] parameters = new object[1];
    					parameters[0] = lpCh;

    					val = Activator.CreateInstance(destInfo.filedInfo.PropertyType, parameters);
    				}
    				else
    				{
    					val = Activator.CreateInstance(destInfo.filedInfo.PropertyType);
    				}

                    destInfo.filedInfo.SetValue(dest, val, null);
                }
                
    			if(IsValueType(sourceInfo.filedInfo))
    			{
    				if(IsValueType(destInfo.filedInfo))
    				{
    					if(destInfo.filedInfo.PropertyType == sourceInfo.filedInfo.PropertyType)
    					{
    						object val = sourceInfo.filedInfo.GetValue(source, null);
    	                    if(val != null)
    	                    {
    	                        destInfo.filedInfo.SetValue(dest, val, null);
    	                        continue;
    	                    }
    					}
    				}
                }
    			else
    			{
    				if(IsStringType(sourceInfo.filedInfo))
    				{
    					if(IsStringType(destInfo.filedInfo))
    					{
    						String val = sourceInfo.filedInfo.GetValue(source, null) as String;
    						
    						destInfo.filedInfo.SetValue(dest, val.Clone(), null);
    					}
    				}
    				else if(IsListType(sourceInfo.filedInfo))
    	            {
    					if(IsListType(destInfo.filedInfo))
    					{
    		                IList sourceList = sourceInfo.filedInfo.GetValue(source, null) as IList;
    		                IList destList = destInfo.filedInfo.GetValue(dest, null) as IList;
    		                if(sourceList != null)
    		                {
    		                    for(int index = 0 ; index < sourceList.Count ; index++)
    		                    {
                                    object temp = null;
                                    if(sourceList.GetType().GetGenericArguments()[0] == typeof(string)
                                       || sourceList.GetType().GetGenericArguments()[0].IsValueType)
                                    {
                                        temp = sourceList[index];
                                    }
                                    else
                                    {
                                        temp = Activator.CreateInstance(destList.GetType().GetGenericArguments()[0]);
                                        AssignDataByFieldName(sourceList[index], temp);
                                    }

                                    destList.Add( temp );
    		                    }
    		                }
    					}
    	            }
    	            else
    	            {
    	                AssignDataByFieldName(sourceInfo.filedInfo.GetValue(source, null), destInfo.filedInfo.GetValue(dest, null));
    	            }
    			}
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(" assign error: FieldIndex is " + (k + 1) + ", FieldName is " + sourceInfo.Name + "   \n: exception string is: " + e.ToString());
            }
        }
    }

    public static MapDataFiledInfo GetMathcedProperty(List<MapDataFiledInfo> source , MapDataFiledInfo destInfo)
    {
        for( int i = 0 ; i < source.Count ; i++ )
        {
            if( destInfo.Name == source[i].Name )
            {
                //if( destInfo.filedInfo.PropertyType == source[i].filedInfo.PropertyType )
                //{
                    return source[i];
                //}
            }
        }

        return null;
    }

    public static bool IsValueType(PropertyInfo pro)
    {
        if(null == pro)
            return true;

        Type tp = pro.PropertyType;

        return tp == typeof(int)
                || tp == typeof(uint)
                || tp == typeof(float) 
                || tp == typeof(long) 
                || tp == typeof(ulong) 
                || tp == typeof(char) 
                || tp == typeof(byte) 
                || tp == typeof(sbyte) 
                || tp == typeof(short) 
                || tp == typeof(ushort) 
                || tp == typeof(bool);
    }

    public static bool IsListType(PropertyInfo pro)
    {
        if(null == pro)
            return false;
        
        return pro.PropertyType.Name == "List`1";
    }

	public static bool IsStringType(PropertyInfo pro)
	{
		if(null == pro)
			return true;
		
		Type tp = pro.PropertyType;
		
		return tp == typeof(string);
	}
}