using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    class Instance
    {
    	protected  Mention mention = null;
	    protected Context context = null;

	public void addFeature(int idx, double val) {
		getFeatureIndex().add(idx);
		featureValue.add(val);
	}

	public boolean containsIndex(int id) {
		return getFeatureIndex().contains(id);
	}

	public TIntList getFeatureIndex() {
		return featureIndex;
	}

	public TDoubleList getFeatureValue() {
		return featureValue;
	}

	public int getIndex(int idx) {
		if (idx > -1 && idx < getFeatureIndex().size()) {
			return getFeatureIndex().get(idx);
		} else {
			return -1;
		}
	}

	public double getValue(int idx) {
		if (idx > -1 && idx < getFeatureIndex().size()) {
			return featureValue.get(idx);
		} else {
			return 0;
		}
	}

	public int length() {
		return getFeatureIndex().size();
	}

	public void removeAt(int idx) {
		if (idx > -1 && idx < getFeatureIndex().size()) {
			getFeatureIndex().removeAt(idx);
			featureValue.removeAt(idx);
		}
	}

	public void setFeatureIndex(TIntList featureIndex) {
		this.featureIndex = featureIndex;
	}

	public void setFeatureValue(TDoubleList featureValue) {
		this.featureValue = featureValue;
	}

	public void setIndex(int i, Integer integer) {
		if (i > -1 && i < getFeatureIndex().size()) {
			getFeatureIndex().set(i, integer);
		}
	}

	public void setLabel(Label l) {
		label = l;
	}

	@Override
	public String toString() {
		StringBuffer sb = new StringBuffer();
		sb.append(label);
		for (int i = 0; i < getFeatureIndex().size(); i++) {
			sb.append(" " + (getFeatureIndex().get(i)) + ":"
					+ featureValue.get(i));
		}
		return sb.toString();
	}

	public String toString(Model m) {
		StringBuffer sb = new StringBuffer();
		sb.append(label);
		for (int i = 0; i < getFeatureIndex().size(); i++) {
			sb.append(" "
					+ m.featureFactory.allFeatures
							.get(getFeatureIndex().get(i)).name + ":"
					+ featureValue.get(i));
		}
		return sb.toString();
	}
    }
}
