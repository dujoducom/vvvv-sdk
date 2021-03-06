﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VVVV.Hosting.Pins.Config;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
    [ComVisible(false)]
	public abstract class SpreadListBase
	{
		protected static int FInstanceCounter = 1;
	}
	
	/// <summary>
	/// base class for spread lists
	/// </summary>
	[ComVisible(false)]
	public abstract class SpreadList<T> : SpreadListBase, ISpread<ISpread<T>>, IDisposable
	{
		protected Pin<T>[] FPins;
		protected IPluginHost FHost;
		protected PinAttribute FAttribute;
		protected IntConfigPin FConfigPin;
		protected int FOffsetCounter;
		
		public SpreadList(IPluginHost host, PinAttribute attribute)
		{
			//store fields
			FHost = host;
			FAttribute = attribute;
			FPins = new Pin<T>[0];
			
			//create config pin
			var att = new ConfigAttribute(FAttribute.Name + " Count");
			att.DefaultValue = 2;
			
			//increment instance Counter and store it as pin offset
			FOffsetCounter = FInstanceCounter++;
			
			FConfigPin = new IntConfigPin(FHost, att);
			FConfigPin.Updated += UpdatePins;
			
			FConfigPin.Update();
		}
		
		public virtual void Dispose()
		{
		    FConfigPin.Updated -= UpdatePins;
		    FConfigPin.Dispose();
		    foreach (var p in FPins)
		        p.Dispose();
		}

		//pin management
		protected void UpdatePins(object sender, EventArgs args)
		{
			var count = FConfigPin[0];
			
			if (count > FPins.Length)
			{
				//store old pins
				var oldPins = FPins;
				
				//create new array
				FPins = new Pin<T>[count];
				
				//copy/create pins
				for (int i = 0; i<count; i++)
				{
					if (i < oldPins.Length)
						FPins[i] = oldPins[i];
					else	
						FPins[i] = CreatePin(i+1);
				}
				
			}
			else if (count < FPins.Length)
			{
				//store old pins
				var oldPins = FPins;
				
				//create new array
				FPins = new Pin<T>[count];
				
				//copy/delete pins
				for (int i = 0; i<oldPins.Length; i++)
				{
					if (i < FPins.Length)
						FPins[i] = oldPins[i];
					else	
					    oldPins[i].Dispose();
				}
			}
		}
		
		//the actual pin creation
		protected abstract Pin<T> CreatePin(int pos);
		
		public ISpread<T> this[int index]
		{
			get
			{
				return FPins[VMath.Zmod(index, FPins.Length)];
			}
			set 
			{
				
			}
		}
		
		object ISpread.this[int index]
        {
            get 
            {
                return this[index];
            }
            set
            {
                this[index] = (ISpread<T>) value;
            }
        }
		
		public int SliceCount 
		{
			get 
			{
				return FPins.Length;
			}
			set 
			{
				
			}
		}

		public IEnumerator<ISpread<T>> GetEnumerator()
		{
			for(int i=0; i<FPins.Length; i++)
				yield return FPins[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
