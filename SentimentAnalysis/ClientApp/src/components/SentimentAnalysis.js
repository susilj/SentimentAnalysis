import React, { Component, Fragment } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';
import { actionCreators } from '../store/SentimentAnalysis';


class SentimentAnalysis extends Component {
  componentDidMount() {
  }
  onChangePredictText = (e) => {
    e.preventDefault();
    const { changePredictText } = this.props;
    changePredictText(e.target.value);
  }
  render() {
    const { predictText, predict, predictResults } = this.props;

    return (
      <div>
        <div className="row">
          <div className="col-12">
          {/*<Link className='btn btn-outline-primary pull-right' to={'/sentimentAnalysis/train'}><span class="badge badge-pill badge-dark" style={{'font-size': '48%'}}>&nbsp;</span>Train</Link>
          <Link className='btn btn-outline-primary pull-right' to={'/sentimentAnalysis/test'}>Test</Link>*/}
            <Link className='btn btn-outline-primary float-right' to={'/configure'}>Configuration <i className="material-icons align-middle">settings</i></Link>
            </div>
        </div>
        <div className="row pt-3">
          <div className="col">
            <div className="input-group mb-3">
              <input type="text" className="form-control" placeholder="Enter text to predict sentiment" value={predictText} onChange={this.onChangePredictText} />
              <div className="input-group-append">
                <input type="button" className="btn btn-outline-secondary" value="Predict" onClick={predict} />
              </div>
            </div>
            {
              predictResults && predictResults.map(result => {
                return (
                  <Fragment>
                    <div>Sentiment: {result.item1.sentimentText}</div>
                    <div>Prediction: {result.item2.prediction ? 'Toxic' : 'Not Toxic'}</div>
                    <div>Probability: {result.item2.probability}</div>
                  </Fragment>
                );
              })
            }
          </div>
        </div>
      </div>
    );
  }
}

export default connect(
  state => state.sentimentAnalysis,
  dispatch => bindActionCreators(actionCreators, dispatch)
)(SentimentAnalysis);
