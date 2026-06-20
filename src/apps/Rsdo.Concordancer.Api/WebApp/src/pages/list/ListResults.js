import { useQuery } from 'react-query';
import { useTranslation } from 'react-i18next';
import { useState } from 'react';

import useSearch from '../../hooks/use-search';
import useDownload from '../../hooks/use-download';
import useHistory from '../../hooks/use-history';

import ResultsLayout from '../../layout/ResultsLayout';
import ResultsHeader from '../../components/results/ResultsHeader';
import ResultsPager from '../../components/results/ResultsPager';
import ResultsExport from '../../components/results/ResultsExport';
import ListItems from '../../components/list/ListItems';
import ListHelp from '../../components/list/ListHelp';
import Spinner from '../../components/shared/Spinner';

const ListResults = () => {
    // Hooks
    const { t } = useTranslation();
    const search = useSearch();
    const history = useHistory();
    const body = search.getSearchRequest();

    const fetchList = () => fetch(`${process.env.PUBLIC_URL}/concordancer/corpus/${search.corpusId}/list/search`, {
        method: 'POST',
        body: JSON.stringify(body),
        headers: {
            'Content-Type': 'application/json'
        }
    }).then((res) => {
        history.saveSearch('list');
        return res.json();
    });

    const { isLoading, isSuccess, isError, data, error } = useQuery(['list', body], fetchList, { keepPreviousData: true, refetchOnWindowFocus: false });
    const [isVisible, setIsVisible] = useState(false);
    const download = useDownload(`${process.env.PUBLIC_URL}/concordancer/corpus/${search.corpusId}/list/export`);

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
            <ResultsLayout source='list' help={<ListHelp />}>
                <div className='row'>
                    <div className='col'>
                        <ResultsHeader query={body.query} onExport={() => setIsVisible(prevIsVisible => !prevIsVisible)} />
                        <ResultsPager total={data.total} offset={data.offset} />
                        {isVisible && <ResultsExport onClose={onExportClose} onConfirm={onExportConfirm} results={data.total} />}
                        <ListItems items={data.items} />
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
            <ResultsLayout source='list' help={<ListHelp />}>
                <div className='row'>
                    <div className='col-lg-10 offset-lg-2'>
                        {content}
                    </div>
                </div>
            </ResultsLayout>
        );
    }
};

export default ListResults;
