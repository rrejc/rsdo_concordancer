import ConcordanceAggregations from '../../components/concordance/ConcordanceAggregations';
import ConcordanceList from '../../components/concordance/ConcordanceList';
import ConcordanceHelp from '../../components/concordance/ConcordanceHelp';
import ResultsHeader from '../../components/results/ResultsHeader';
import ResultsPager from '../../components/results/ResultsPager';
import ResultsExport from '../../components/results/ResultsExport';
import useHistory from '../../hooks/use-history';
import useSearch from '../../hooks/use-search';
import useDownload from '../../hooks/use-download';
import ResultsLayout from '../../layout/ResultsLayout';
import Spinner from '../../components/shared/Spinner';
import { useQuery } from 'react-query';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';

const ConcordanceResults = () => {
    // Hooks
    const { t } = useTranslation();
    const search = useSearch();    
    const history = useHistory();

    const body = search.getSearchRequest();

    const fetchConcordances = () => fetch(`${process.env.PUBLIC_URL}/concordancer/corpus/${search.corpusId}/concordances/search`, {
        method: 'POST',
        body: JSON.stringify(body),
        headers: {
            'Content-Type': 'application/json'
        }
    }).then((res) => {
        history.saveSearch('concordance');
        return res.json();
    });

    const { isLoading, isSuccess, isError, data, error } = useQuery(['concordances', body], fetchConcordances, { keepPreviousData: true, refetchOnWindowFocus: false });
    const [isVisible, setIsVisible] = useState(false);
    const download = useDownload(`${process.env.PUBLIC_URL}/concordancer/corpus/${search.corpusId}/concordances/export`);

    const onExportClose = () => {
        setIsVisible(false);
    };

    const onExportConfirm = (type, rows) => {
        setIsVisible(false);
        body.type = type;
        body.rows = rows;
        download.start(body);
    };

    if (!isLoading && isSuccess && data.items.length > 0) {
        return (
            <ResultsLayout source='concordance' help={<ConcordanceHelp />}>
                <div className='row'>
                    <div className='col-lg-2'>
                        <ConcordanceAggregations aggregations={data.aggregations} lemmasAlternateSearch={data.lemmasAlternateSearch} />
                    </div>
                    <div className='col-lg-10'>
                        <ResultsHeader query={body.query} onExport={() => setIsVisible(prevIsVisible => !prevIsVisible)} />
                        <ResultsPager total={data.total} offset={data.offset} />
                        {isVisible && <ResultsExport onClose={onExportClose} onConfirm={onExportConfirm} results={data.total} />}
                        <ConcordanceList items={data.items} />
                    </div>
                </div>
            </ResultsLayout>
        );
    } else {
        let content = <></>;
        if (isLoading) {
            content = <Spinner></Spinner>
        } else if (isError) {
            content = <p>{t('shared.searchError')}</p>
        } else if (!isLoading && data && data.items.length === 0) {
            content = <p>{t('shared.noResults')}</p>
        }
        return (
            <ResultsLayout source='concordance' help={<ConcordanceHelp />}>
                <div className='row'>
                    <div className='col-lg-10 offset-lg-2'>
                        {content}
                    </div>
                </div>
            </ResultsLayout>
        );
    };
};

export default ConcordanceResults;
