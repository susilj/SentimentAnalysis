import React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
//import Home from './components/Home';
import Configure from './components/Configure';
//import FetchData from './components/FetchData';
import SentimentAnalysis from './components/SentimentAnalysis';

export default () => (
  <Layout>
    {/*<Route exact path='/' component={Home} />
    <Route path='/counter' component={Counter} />
    <Route path='/fetch-data/:startDateIndex?' component={FetchData} />
    <Route path='/sentimentanalysis' component={SentimentAnalysis} />*/}
    <Route exact path='/' component={SentimentAnalysis} />
    <Route exact path='/configure' component={Configure} />
  </Layout>
);
