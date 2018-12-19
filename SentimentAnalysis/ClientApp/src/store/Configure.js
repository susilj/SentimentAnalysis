const trainDataFileListRequest = 'TRAIN_DATA_FILE_LIST_REQUEST';
const trainDataFileListResponse = 'TRAIN_DATA_FILE_LIST_RESPONSE';
const downloadTrainDataFileRequest = 'DOWNLOAD_TRAIN_DATA_FILE_REQUEST';
const downloadTrainDataFileResponse = 'DOWNLOAD_TRAIN_DATA_FILE_RESPONSE';
const setUploadFile = 'SET_UPLOAD_FILE';
const uploadFileRequest = 'UPLOAD_FILE_REQUEST';
const uploadFileResponse = 'UPLOAD_FILE_RESPONSE';
const trainUploadedDataRequest = 'TRAIN_UPLOADED_DATE_REQUEST';
const trainUploadedDataResponse = 'TRAIN_UPLOADED_DATE_RESPONSE';
const initialState = {
  trainDataFileList: [],
  uploadFile: {},
  uploadDefaultLabel: 'Choose file',
  trainData: false,
};

initialState.uploadLabelText = initialState.uploadDefaultLabel;

export const actionCreators = {
  listFiles: () => async (dispatch) => {
    dispatch({ type: trainDataFileListRequest });
    const url = `api/SentimentDataTrain/ListTrainDataFiles`;
    const response = await fetch(url);
    const trainDataFiles = await response.json();
    
    dispatch({ type: trainDataFileListResponse, trainDataFiles });
  },
  downloadFile: (fileName) => async (dispatch) => {
    dispatch({ type: downloadTrainDataFileRequest });

    const url = `api/SentimentDataTrain/DownloadFile/${fileName}`;
    const response = await fetch(url);
    const fileData = await response.text();

    const downloadUrl = window.URL.createObjectURL(new Blob([fileData]));
    const link = document.createElement('a');
    link.href = downloadUrl;
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();

    dispatch({ type: downloadTrainDataFileResponse });
  },
  setUploadFile: (file) => async (dispatch) => {
    dispatch({ type: setUploadFile, file });
  },
  uploadFile: () => async (dispatch, getState) => {
    dispatch({ type: uploadFileRequest });

    const fileData = getState().configure.uploadFile;

    const url = 'api/SentimentDataTrain/UploadFile';

    const files = new FormData();
    files.append("file", fileData);

    const response = await fetch(url, {
      method: 'POST',
      body: files
    });
    console.log(response);
    const responseData = await response.json();

    dispatch({ type: uploadFileResponse, fileName: fileData.name });
  },
  trainModel: () => async (dispatch) => {
    dispatch({ type: trainUploadedDataRequest });

    const url = 'api/SentimentDataTrain/Train';

    const response = await fetch(url);
    console.log(response);
    const responseData = await response.json();

    dispatch({ type: trainUploadedDataResponse });
  }
};

export const reducer = (state, action) => {
  state = state || initialState;

  if (action.type === trainDataFileListRequest) {
    return { ...state };
  }

  if (action.type === trainDataFileListResponse) {
    return { ...state, trainDataFileList: action.trainDataFiles };
  }

  if (action.type === downloadTrainDataFileRequest) {
    return { ...state };
  }

  if (action.type === downloadTrainDataFileResponse) {
    return { ...state };
  }

  if (action.type === setUploadFile) {
    return { ...state, uploadFile: action.file, uploadLabelText: action.file.name };
  }

  if (action.type === uploadFileRequest) {
    return { ...state };
  }

  if (action.type === uploadFileResponse) {
    return { ...state, trainDataFileList: [...state.trainDataFileList, action.fileName], uploadLabelText: initialState.uploadDefaultLabel, trainData: true };
  }

  if (action.type === trainUploadedDataRequest) {
    return { ...state };
  }

  if (action.type === trainUploadedDataResponse) {
    return { ...state, trainData: false };
  }

  return state;
};
