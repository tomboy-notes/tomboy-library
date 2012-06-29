// 
//  SearchEngine.cs
//  
//  Author:
//       Robert Nordan <rpvn@robpvn.net>
// 
//  Copyright (c) 2012 Robert Nordan
// 
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
// 
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
using System;
using System.Collections.Generic;
using System.Linq;


namespace Tomboy
{
	public class SearchEngine
	{
		public SearchEngine ()
		{
		}

		public static Dictionary<string, Note>  SearchTitles (string searchTerm, Dictionary<string, Note> searchSource)
		{
			return searchSource.Where (d => d.Value.Title.ToLowerInvariant ().Contains (searchTerm)).ToDictionary (d => d.Key, d => d.Value);
		}

		public static Dictionary<string, Note>  SearchContent (string searchTerm, Dictionary<string, Note> searchSource)
		{
			return searchSource.Where (d => (d.Value.Title.ToLowerInvariant ().Contains (searchTerm)) ||
			                           (d.Value.Text.ToLowerInvariant ().Contains (searchTerm))
			                           ).ToDictionary (d => d.Key, d => d.Value);
		}
	}
}

