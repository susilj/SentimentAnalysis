const predictRequestType = 'PREDICT_TEXT_REQUEST';
const predictResponseType = 'PREDICT_TEXT_RESPONSE';
const predictTextChange = 'PREDICT_TEXT_CHANGE';
const initialState = {
  predictText: '',
  predictResults: []
};

export const actionCreators = {
  predict: () => async (dispatch, getState) => {
    const predictText = getState().sentimentAnalysis.predictText;

    if ('' === predictText) {
      return;
    }

    dispatch({ type: predictRequestType });

    const url = `api/SentimentDataTrain/Predict`;
    const response = await fetch(url,
    {
      method: 'post',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ "PredictionText": predictText })
    });
    const predictionResult = await response.json();

    dispatch({ type: predictResponseType, predictionResult });
  },
  changePredictText: value => (dispatch) => dispatch({ type: predictTextChange, value })
};

export const reducer = (state, action) => {
  state = state || initialState;

  if (action.type === predictRequestType) {
    return { ...state, predictText: action.predictText };
  }

  if (action.type === predictResponseType) {
    return { ...state, predictResults: action.predictionResult };
  }

  if (action.type === predictTextChange) {
    return { ...state, predictText: action.value };
  }

  return state;
};
