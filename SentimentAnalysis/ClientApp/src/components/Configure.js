import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
//import classNames from 'classnames'

import { actionCreators } from '../store/Configure';

class Configure extends Component {
  componentDidMount() {
    this.props.listFiles();
  }
  onHandleDownload = (e, fileName) => {
    e.preventDefault();

    this.props.downloadFile(fileName);
  }
  onHandleUpload = (e) => {
    e.preventDefault();
    const file = e.target.files[0]
    this.props.setUploadFile(file);
  }
  render() {
    const { trainDataFileList, setUploadFile, trainData, trainModel } = this.props;
    console.log(this.props);
    return (
      <div>
        <div className="row">
          <div className="col-12">
            {/*<Link className='btn btn-outline-primary pull-right' to={'/sentimentAnalysis/train'}><span class="badge badge-pill badge-dark" style={{'font-size': '48%'}}>&nbsp;</span>Train</Link>
          <Link className='btn btn-outline-primary pull-right' to={'/sentimentAnalysis/test'}>Test</Link>*/}
            <input type="button" disabled={!trainData} className='btn btn-success float-right' onClick={trainModel} value="Train" />
          </div>
        </div>
        <h1>Data file list</h1>
        <ul className="list-group">
          {
            trainDataFileList && trainDataFileList.map((file, index) => {
              const fileKey = `${file}_${index}`;
              return (
                <li className="list-group-item d-flex justify-content-between align-items-center" key={fileKey}>
                  {file}
                  <span className="badge badge-primary badge-pill"></span>
                  <button type="button" className="btn btn-link" onClick={() => this.props.downloadFile(file)}>Download</button>
                </li>
              );
            })
          }
        </ul>
        <div className="pt-5">
          <div className="custom-file">
            <input type="file" className="custom-file-input" id="File"
              onChange={(e) => setUploadFile(e.target.files[0])}
            />
            <label className="custom-file-label" htmlFor="File">{this.props.uploadLabelText}</label>
            <button type="button" className="btn btn-link" onClick={this.props.uploadFile}>Upload</button>
          </div>
        </div>
      </div>
    );
  }
}

export default connect(
  state => state.configure,
  dispatch => bindActionCreators(actionCreators, dispatch)
)(Configure);
