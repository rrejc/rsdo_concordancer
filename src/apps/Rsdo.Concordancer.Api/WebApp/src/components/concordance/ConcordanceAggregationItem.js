import { Link } from 'react-router-dom';
import useSearch from '../../hooks/use-search';
import styles from './ConcordanceAggregationItem.module.scss';
import { useTranslation } from 'react-i18next';

import selectIcon from '../../assets/chevron-right.svg';
import selectedIcon from '../../assets/square-checkbox-solid.svg';

const ConcordanceAggregationItem = (props) => {
    const { t } = useTranslation();
    const search = useSearch();
    const isSelected = search.isFiltered('TextIds', props.filterKey) === true;
    const link = isSelected ? search.getClearFilterLink('TextIds', props.filterKey) : search.getFilterLink('TextIds', props.filterKey);

    let icon;
    if (isSelected) {
        icon = <img src={selectedIcon} alt={t('shared.unselect')} />;
    } else  {
        icon = <img src={selectIcon} alt={t('shared.select')} />;
    }
    
    return (
        <li className={styles.aggregationItem}><Link to={link}>{icon}<span className={styles.title}>{props.title}</span><span className={styles.count}>{props.count}</span></Link></li>
    );
};

export default ConcordanceAggregationItem;
