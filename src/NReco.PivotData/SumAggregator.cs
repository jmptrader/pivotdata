﻿/*
 *  Copyright 2015-2020 Vitaliy Fedorchenko (nrecosite.com)
 *
 *  Licensed under PivotData Source Code Licence (see LICENSE file).
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS 
 *  OF ANY KIND, either express or implied.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace NReco.PivotData {

	/// <summary>
	/// Implements a sum aggregator
	/// </summary>
	/// <remarks>
	/// SumAggregator can be used only with numeric values that may be coverted to System.Decimal.
	/// </remarks>	
	public class SumAggregator : IAggregator {

		decimal total = 0;
		uint count = 0;
		string field;

		public SumAggregator(string f) {
			field = f;
		}

		public SumAggregator(string f, object state) : this(f) {
			var stateArr = state as object[];
			if (stateArr==null || stateArr.Length!=2)
				throw new InvalidOperationException("Invalid state, expected array [uint count, decimal value]");
			count = Convert.ToUInt32(stateArr[0]);
			total = Convert.ToDecimal(stateArr[1]);
		}

		public void Push(object r, Func<object,string,object> getValue) {
			var v = ConvertHelper.ConvertToDecimal(getValue(r,field), Decimal.MinValue);
			if (v!=Decimal.MinValue) {
				total += v;
				count++;
			}
		}

		public object Value {
			get { return total; }
		}

		public uint Count {
			get { return count; }
		}

		public void Merge(IAggregator aggr) {
			var sumAggr = aggr as SumAggregator;
			if (sumAggr==null)
				throw new ArgumentException("aggr");
			count += sumAggr.count;
			total += sumAggr.total;
		}

		public object GetState() {
			return new object[]{count, total};
		}
	}

	/// <summary>
	/// <see cref="SumAggregator"/> factory component
	/// </summary>
	public class SumAggregatorFactory : IAggregatorFactory {

		public string Field { 
			get { return fld; }
		}

		string fld;

		public SumAggregatorFactory(string field) {
			fld = field;
		}

		public IAggregator Create() {
			return new SumAggregator(fld);
		}

		public IAggregator Create(object state) {
			return new SumAggregator(fld, state);
		}

		public override bool Equals(object obj) {
			var aggrFactory = obj as SumAggregatorFactory;
			if (aggrFactory==null)
				return false;
			return aggrFactory.fld==fld;
		}

		public override string ToString() {
			return String.Format("Sum of {0}", Field);
		}

	}
}
